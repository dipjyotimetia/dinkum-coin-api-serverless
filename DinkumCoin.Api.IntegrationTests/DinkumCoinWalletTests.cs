using System;
using Xunit;
using DinkumCoin.Api.Client;

namespace DinkumCoin.Api.IntegrationTests
{
    public class DinkumCoinWalletTests
    {
        [Fact]
        public void GivenAtLeastOneWalletExists_WhenCallWalletsResourse_GetWalletsAsArrayOfKeyValuePairs()
        {
            // Arrange
            var baseUri = "https://sywq3pqw4c.execute-api.ap-southeast-2.amazonaws.com/dev/";
            var client = new DinkumCoinApiClient(baseUri);



            // Act
            var response = client.GetAllWallets();


            // Assert
            Assert.NotNull(response);
        }
    }
}
