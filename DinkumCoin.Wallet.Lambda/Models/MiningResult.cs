using System;
using DinkumCoin.Core.Models;

namespace DinkumCoin.Wallet.Lambda.Models
{
    public class MiningResult
    {
        public bool CoinCreated { get; set; }
        public Coin NewCoin {get; set;}
    }
}
