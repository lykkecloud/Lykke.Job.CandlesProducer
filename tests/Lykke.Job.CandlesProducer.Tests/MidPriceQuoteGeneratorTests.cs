using System;
using Lykke.Domain.Prices.Model;
using Lykke.Job.CandlesProducer.Core.Services.Candles;
using Lykke.Job.CandlesProducer.Services.Candles;
using Xunit;

namespace Lykke.Job.CandlesProducer.Tests
{
    public class MidPriceQuoteGeneratorTests
    {
        private readonly IMidPriceQuoteGenerator _generator;

        public MidPriceQuoteGeneratorTests()
        {
            _generator = new MidPriceQuoteGenerator();
        }

        [Fact]
        public void Bid_and_ask_quotes_generates_mid_quote()
        {
            // Arrange
            var date1 = new DateTime(2017, 06, 23, 12, 56, 00);
            var date2 = new DateTime(2017, 06, 23, 12, 56, 30);

            // Act
            _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1, Timestamp = date1 }, 3);
            var mid = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 2, Timestamp = date2 }, 3);

            // Assert
            Assert.NotNull(mid);
            Assert.Equal("EURUSD", mid.AssetPair);
            Assert.False(mid.IsBuy);
            Assert.Equal(1.5, mid.Price);
            Assert.Equal(date2, mid.Timestamp);
        }


        [Fact]
        public void Sequental_bid_and_ask_quotes_generates_new_mid_quote()
        {
            // Arrange
            var date1 = new DateTime(2017, 06, 23, 12, 56, 00);
            var date2 = new DateTime(2017, 06, 23, 12, 56, 30);
            var date3 = new DateTime(2017, 06, 23, 12, 57, 00);
            var date4 = new DateTime(2017, 06, 23, 12, 58, 00);

            // Act
            _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1, Timestamp = date1 }, 3);
            _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 2, Timestamp = date2 }, 3);
            var mid2 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 3, Timestamp = date3 }, 3);
            var mid3 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 2, Timestamp = date4 }, 3);

            // Assert
            Assert.NotNull(mid2);
            Assert.Equal("EURUSD", mid2.AssetPair);
            Assert.False(mid2.IsBuy);
            Assert.Equal(2, mid2.Price);
            Assert.Equal(date3, mid2.Timestamp);

            Assert.NotNull(mid3);
            Assert.Equal("EURUSD", mid3.AssetPair);
            Assert.False(mid3.IsBuy);
            Assert.Equal(2.5, mid3.Price);
            Assert.Equal(date4, mid3.Timestamp);
        }

        [Fact]
        public void Mid_quote_price_is_rounded()
        {
            // Act
            _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1.123, Timestamp = DateTime.UtcNow }, 2);
            var mid = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 2, Timestamp = DateTime.UtcNow }, 2);

            // Assert
            Assert.Equal(1.56, mid.Price);
        }

        [Fact]
        public void Bid_only_quotes_not_generates_mid_quote()
        {
            // Act
            var mid1 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 1, Timestamp = DateTime.UtcNow }, 3);
            var mid2 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 1, Timestamp = DateTime.UtcNow }, 3);
            var mid3 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = true, Price = 1, Timestamp = DateTime.UtcNow }, 3);

            // Assert
            Assert.Null(mid1);
            Assert.Null(mid2);
            Assert.Null(mid3);
        }

        [Fact]
        public void Ask_only_quotes_not_generates_mid_quote()
        {
            // Act
            var mid1 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1, Timestamp = DateTime.UtcNow }, 3);
            var mid2 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1, Timestamp = DateTime.UtcNow }, 3);
            var mid3 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1, Timestamp = DateTime.UtcNow }, 3);

            // Assert
            Assert.Null(mid1);
            Assert.Null(mid2);
            Assert.Null(mid3);
        }

        [Fact]
        public void Different_asset_pair_quotes_not_generates_mid_quote()
        {
            // Act
            var mid1 = _generator.TryGenerate(new Quote { AssetPair = "EURUSD", IsBuy = false, Price = 1, Timestamp = DateTime.UtcNow }, 3);
            var mid2 = _generator.TryGenerate(new Quote { AssetPair = "USDCHF", IsBuy = true, Price = 1, Timestamp = DateTime.UtcNow }, 3);
            var mid3 = _generator.TryGenerate(new Quote { AssetPair = "USDRUB", IsBuy = false, Price = 1, Timestamp = DateTime.UtcNow }, 3);

            // Assert
            Assert.Null(mid1);
            Assert.Null(mid2);
            Assert.Null(mid3);
        }
    }
}
