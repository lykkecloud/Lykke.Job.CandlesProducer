using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Domain.Prices;
using Lykke.Job.CandlesProducer.Core.Domain.Candles;
using Lykke.Job.CandlesProducer.Core.Services.Candles;

namespace Lykke.Job.CandlesProducer.Services.Candles
{
    [UsedImplicitly]
    public class CandlesGenerator : ICandlesGenerator
    {
        private readonly ILog _log;
        private ConcurrentDictionary<string, LinkedList<Candle>> _candles;
        
        public CandlesGenerator(ILog log)
        {
            _log = log;
            _candles = new ConcurrentDictionary<string, LinkedList<Candle>>();
        }

        public CandleUpdateResult Update(string assetPair, DateTime timestamp, double price, double volume, PriceType priceType, TimeInterval timeInterval)
        {
            var key = GetKey(assetPair, timeInterval, priceType);
            Candle oldCandle = null;
            Candle newCandle = null;

            _candles.AddOrUpdate(key,
                addValueFactory: k =>
                {
                    var candles = new LinkedList<Candle>();

                    newCandle = Candle.Create(assetPair, timestamp, price, volume, priceType, timeInterval);

                    candles.AddFirst(newCandle);

                    return candles;
                },
                updateValueFactory: (k, candles) =>
                {
                    var candleTimestamp = timestamp.RoundTo(timeInterval);

                    if (candles.Any() && candleTimestamp < candles.First.Value.Timestamp)
                    {
                        // Given data is older then oldest of the cached candles.
                        // Nothing to update here, so just creates the candles from the
                        // given data and return it.

                        _log.WriteWarningAsync(
                            nameof(CandlesGenerator),
                            nameof(Update),
                            new
                            {
                                assetPair = assetPair,
                                timestamp = timestamp,
                                price = price,
                                volume = volume,
                                oldestCachedCandle = candles.First.Value
                            }.ToJson(),
                            "Candle is to old to update. New single tick candle will be returned as the result").Wait();

                        newCandle = Candle.Create(assetPair, timestamp, price, volume, priceType, timeInterval);

                        return candles;
                    }

                    // Given data is not older then oldest of the cached candles.

                    for (var item = candles.First; item.Next != null; item = item.Next)
                    {
                        var candle = item.Value;

                        if (candleTimestamp == candle.Timestamp)
                        {
                            // Candle matches exactly - updating it

                            oldCandle = item.Value;
                            newCandle = oldCandle.Update(timestamp, price, volume);

                            item.Value = newCandle;

                            return candles;
                        }

                        if (candleTimestamp > candle.Timestamp)
                        {
                            // We don't find candle that matches exactly
                            // and curent candle is older than given data,
                            // so insert new candle just before the current candle

                            newCandle = Candle.Create(assetPair, timestamp, price, volume, priceType, timeInterval);

                            candles.AddBefore(item, newCandle);

                            TruncateTooBigCache(timeInterval, candles);

                            return candles;
                        }
                    }

                    // Given data is the newer than the newest cached candle, so add new candle to the tail

                    newCandle = Candle.Create(assetPair, timestamp, price, volume, priceType, timeInterval);

                    candles.AddLast(newCandle);

                    TruncateTooBigCache(timeInterval, candles);

                    return candles;
                });

            if (newCandle == null)
            {
                throw new InvalidOperationException("Now candle was created");
            }

            return new CandleUpdateResult(newCandle, oldCandle, !newCandle.Equals(oldCandle));
        }

        public void Undo(CandleUpdateResult candleUpdateResult)
        {
            if (!candleUpdateResult.WasChanged)
            {
                return;
            }

            var candle = candleUpdateResult.Candle;
            var oldCandle = candleUpdateResult.OldCandle;
            var key = GetKey(candle.AssetPairId, candle.TimeInterval, candle.PriceType);

            // Key should be presented in the dictionary, since Update was called before and keys are never removed

            _candles.AddOrUpdate(
                key,
                addValueFactory: k => throw new InvalidOperationException("Key should be already presented in the dictionary"),
                updateValueFactory: (k, candles) =>
                {
                    for (var item = candles.First; item.Next != null; item = item.Next)
                    {
                        var cachedCandle = item.Value;

                        if (cachedCandle.Timestamp == candle.Timestamp)
                        {
                            if (cachedCandle.LastUpdateTimestamp == candle.LastUpdateTimestamp)
                            {
                                // Candle wasn't changed between Update and Undo call, so we can just revert to the old candle

                                if (oldCandle == null)
                                {
                                    candles.Remove(item);
                                }
                                else
                                {
                                    item.Value = oldCandle;
                                }

                                return candles;
                            }

                            // Candle was changed between Update and Undo call, so we should revert only addition operations

                            var oldVolume = oldCandle?.TradingVolume ?? 0;
                            var volumeToUndo = candle.TradingVolume - oldVolume;

                            item.Value = cachedCandle.SubstractVolume(volumeToUndo);

                            return candles;
                        }    
                    }

                    // Candle was already evicted from the cache, so nothing to undo at all

                    return candles;
                });
        }

        public ImmutableDictionary<string, ImmutableList<ICandle>> GetState()
        {
            return _candles.ToImmutableDictionary(i => i.Key, i => i.Value.Cast<ICandle>().ToImmutableList());
        }

        public void SetState(ImmutableDictionary<string, ImmutableList<ICandle>> state)
        {
            if (_candles.Count > 0)
            {
                throw new InvalidOperationException("Candles generator state already not empty");
            }

            var keyValuePairs = state.Select(i => KeyValuePair.Create(i.Key, new LinkedList<Candle>(i.Value.Select(Candle.Copy))));

            _candles = new ConcurrentDictionary<string, LinkedList<Candle>>(keyValuePairs);
        }

        public string DescribeState(ImmutableDictionary<string, ImmutableList<ICandle>> state)
        {
            return $"Candles count: {state.Sum(i => i.Value.Count)}";
        }

        private static string GetKey(string assetPairId, TimeInterval timeInterval, PriceType priceType)
        {
            return $"{assetPairId.Trim().ToUpper()}-{priceType}-{timeInterval}";
        }

        private static void TruncateTooBigCache(TimeInterval timeInterval, LinkedList<Candle> candles)
        {
            if (candles.Count > Constants.PublishedIntervalsHistoryDepth[timeInterval])
            {
                // Cached candles count limit is exceeded - remove the oldest cached candle

                candles.RemoveFirst();
            }
        }
    }
}
