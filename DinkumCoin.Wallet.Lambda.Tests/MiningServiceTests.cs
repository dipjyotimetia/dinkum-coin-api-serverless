using System;
using DinkumCoin.Core.Models;
using DinkumCoin.Wallet.Lambda.Contracts;
using DinkumCoin.Wallet.Lambda.Services;
using Moq;
using Xunit;

namespace DinkumCoin.Wallet.Lambda.Tests
{
    public class MiningServiceTests
    {
        [Fact]
        public void GivenMiningAttemptSuccessful_ResultIncludesNewlyMintedCoin()
        {
            // Arrange
            var mockMathService = new Mock<IMathService>();
            mockMathService.Setup(x => x.IsPrime(It.IsAny<int>())).Returns(() => true);
            var sut = new MiningService(mockMathService.Object);

            // Act
            var actualResult = sut.AttemptMineNewCoin();

            // Assert
            Assert.True(actualResult.CoinCreated);
            Assert.NotNull(actualResult.NewCoin);
            Assert.IsType<Coin>(actualResult.NewCoin);
        }

        [Fact]
        public void GivenMiningAttemptFails_ResultDoesNotIncludeCoin()
        {
            // Arrange
            var mockMathService = new Mock<IMathService>();
            mockMathService.Setup(x => x.IsPrime(It.IsAny<int>())).Returns(() => false);
            var sut = new MiningService(mockMathService.Object);

            // Act
            var actualResult = sut.AttemptMineNewCoin();

            // Assert
            Assert.False(actualResult.CoinCreated);
            Assert.Null(actualResult.NewCoin);
        }
    }
}
