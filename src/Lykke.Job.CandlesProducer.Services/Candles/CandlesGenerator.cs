using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Job.CandlesProducer.Contract;
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

        public CandleUpdateResult UpdatePrice(string assetPair, DateTime timestamp, double price, CandlePriceType priceType, CandleTimeInterval timeInterval)
        {
            return Update(assetPair, timestamp, priceType, timeInterval,
                createNewCandle: () => Candle.CreateWithPrice(assetPair, timestamp, price, priceType, timeInterval),
                updateCandle: oldCandle => oldCandle.UpdatePrice(timestamp, price),
                getLoggingContext: candles => new
                {
                    assetPair = assetPair,
                    timestamp = timestamp,
                    oldestCachedCandle = candles.First.Value
                });
        }

        public CandleUpdateResult UpdateTradingVolume(string assetPair, DateTime timestamp, double volume, CandlePriceType priceType,
            CandleTimeInterval timeInterval)
        {
            return Update(assetPair, timestamp, priceType, timeInterval,
                createNewCandle: () => Candle.CreateWithTradingVolume(assetPair, timestamp, volume, priceType, timeInterval), 
                updateCandle: oldCandle => oldCandle.UpdateTradingVolume(timestamp, volume),
                getLoggingContext: candles => new
                {
                    assetPair = assetPair,
                    timestamp = timestamp,
                    volume = volume,
                    oldestCachedCandle = candles.First.Value
                });
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
                            if (cachedCandle.LatestChangeTimestamp == candle.LatestChangeTimestamp)
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

        private CandleUpdateResult Update(
            string assetPair,
            DateTime timestamp,
            CandlePriceType priceType,
            CandleTimeInterval timeInterval,
            Func<Candle> createNewCandle,
            Func<Candle, Candle> updateCandle,
            Func<LinkedList<Candle>, object> getLoggingContext)
        {
            var key = GetKey(assetPair, timeInterval, priceType);
            Candle oldCandle = null;
            Candle newCandle = null;
            var isLatestCandle = false;

            _candles.AddOrUpdate(key,
                addValueFactory: k =>
                {
                    var candles = new LinkedList<Candle>();

                    newCandle = createNewCandle();
                    isLatestCandle = true;

                    candles.AddFirst(newCandle);

                    return candles;
                },
                updateValueFactory: (k, candles) =>
                {
                    // Candles is ordered by the Timestamp

                    var candleTimestamp = timestamp.TruncateTo(timeInterval);

                    // Common cases:
                    // 1. lastCandle should be update
                    // 2. new candle should be added to the tail
                    // so start search from the end

                    for (var item = candles.Last; item != null; item = item.Previous)
                    {
                        var candle = item.Value;

                        if (candleTimestamp == candle.Timestamp)
                        {
                            // Candle matches exactly - updating it

                            oldCandle = item.Value;
                            newCandle = updateCandle(oldCandle);
                            isLatestCandle = item == candles.Last;

                            item.Value = newCandle;

                            return candles;
                        }

                        if (candleTimestamp > candle.Timestamp)
                        {
                            // We don't find the candle that matches exactly yet,
                            // but curent given data is newer than the current candle,
                            // so insert new candle just after the current candle

                            newCandle = createNewCandle();
                            isLatestCandle = item == candles.Last;

                            candles.AddAfter(item, newCandle);

                            PruneCache(candles);

                            return candles;
                        }

                        // Given data is older then the current candle, so
                        // continue searching of the exactly matched or older candler
                    }

                    // Given data is older then the oldest of the cached candles.

                    if (ShouldBeCached(candles, timestamp))
                    {
                        // Cache not filled yet, so saves the candle

                        newCandle = createNewCandle();

                        candles.AddFirst(newCandle);
                    }
                    else
                    {
                        // Nothing to update here and no candle can be returned
                        // since we can't obtain full candle state

                        _log.WriteWarningAsync(
                            nameof(CandlesGenerator),
                            nameof(UpdatePrice),
                            getLoggingContext(candles).ToJson(),
                            "Incoming data is to old to update the candle. No candle will be generated").Wait();
                    }

                    return candles;
                });

            // Candles without prices shouldn't be produced
            return newCandle == null || !newCandle.HasPrices
                ? CandleUpdateResult.Empty
                : new CandleUpdateResult(
                    newCandle,
                    oldCandle,
                    wasChanged: !newCandle.Equals(oldCandle),
                    isLatestCandle: isLatestCandle,
                    isLatestChange: oldCandle == null || newCandle.LatestChangeTimestamp >= oldCandle.LatestChangeTimestamp);
        }

        private static string GetKey(string assetPairId, CandleTimeInterval timeInterval, CandlePriceType priceType)
        {
            return $"{assetPairId.Trim().ToUpper()}-{priceType}-{timeInterval}";
        }

        private static void PruneCache(LinkedList<Candle> candles)
        {
            if (!ShouldBeCached(candles, candles.First.Value.Timestamp))
            {
                candles.RemoveFirst();
            }
        }

        private static bool ShouldBeCached(LinkedList<Candle> candles, DateTime timestampToCheck)
        {
            // Stores at least half a day and not less than 2 candles for bigger intervals, 
            // to let quotes and trades meet each other in the candles cache

            if (candles.Count > 2)
            {
                var depth = candles.Last.Value.Timestamp - timestampToCheck;

                if (depth > TimeSpan.FromHours(12))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
