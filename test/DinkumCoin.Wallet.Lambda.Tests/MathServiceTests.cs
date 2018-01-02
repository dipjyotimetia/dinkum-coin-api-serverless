using System;
namespace DinkumCoin.Wallet.Lambda.Tests
{
    using DinkumCoin.Wallet.Lambda.Services;
    using Xunit;
    public class MathServiceTests
    {
        [Fact]
        public void GivenValidPrimeNumbers_WhenIsPrimeMethodCalledReturnsTrue()
        {
            // Arrange
            var mathService = new MathService();
            int validPrimeNum = 10061;

            // Act
            var actual = mathService.IsPrime(validPrimeNum);


            // Assert
            Assert.True(actual);
        }


        [Fact]
        public void GivenNonPrimeNumbers_WhenIsPrimeMethodCalledReturnsFalse()
        {
            // Arrange
            var mathService = new MathService();
            int validPrimeNum = 2500;

            // Act
            var actual = mathService.IsPrime(validPrimeNum);


            // Assert
            Assert.False(actual);
        }
    }
}
