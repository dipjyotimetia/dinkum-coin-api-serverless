using System;
using DinkumCoin.Core.Models;
using DinkumCoin.Wallet.Lambda.Contracts;
using DinkumCoin.Wallet.Lambda.Models;
using DinkumCoin.Wallet.Lambda.Util;

namespace DinkumCoin.Wallet.Lambda.Services
{
    public class MiningService : IMiningService
    {

        private IMathService _mathService { get; set; }

        public MiningService(IMathService mathService)
        {
            _mathService = mathService;
        }

        public MiningResult AttemptMineNewCoin()
        {
            
            int candidate = NumberHelpers.GenerateRandomNumber(0, 1000);

            bool success = _mathService.IsPrime(candidate);

            if (!success)
            {
                return new MiningResult() { CoinCreated = false };
            } 
            else
            {
                var newCoin = new Coin() { CreationDate = DateTime.Now, Id = Guid.NewGuid() };
                return new MiningResult() { CoinCreated = true, NewCoin = newCoin };
            }
        }
    }
}
