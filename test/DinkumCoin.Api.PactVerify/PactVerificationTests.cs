using System;
using Xunit;
using DinkumCoin.Api.Client;
using Xunit.Abstractions;
using DinkumCoin.Api.PactVerify.Framework;
using System.Collections.Generic;
using PactNet.Infrastructure.Outputters;

using PactNet;
using System.IO;
using DinkumCoin.Api.PactVerify;

namespace DinkumCoin.Api.PactTests
{
    public class PactVerificationTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private WalletTestFixture _testFixture;

        public PactVerificationTests( ITestOutputHelper output)
        {
            _output = output;
            _testFixture = new WalletTestFixture();
        }

        [Fact]
        public void EnsureDinkumCoinApiHonoursPactWithConsumer()
        {
            //Arrange
            const string serviceUri = "https://8lyhztzwh3.execute-api.ap-southeast-2.amazonaws.com/Dev";
            var config = new PactVerifierConfig
            {
                Outputters = new List<IOutput>
                {
                    new XUnitOutput(_output)
                }
            };



            IPactVerifier pactVerifier = new PactVerifier(config);
            pactVerifier
              //  .ProviderState($"{serviceUri}/provider-states")
                .ServiceProvider("dinkum-coin-api", serviceUri)
                .HonoursPactWith("dinkum-coin-web")
                .PactUri($"pacts/dinkum-coin-web-dinkum-coin-api.json")
                .Verify();
            
        }


        public virtual void Dispose()
        {
            
        }
    }
    



}
