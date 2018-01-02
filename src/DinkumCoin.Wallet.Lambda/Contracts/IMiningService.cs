using System;
using DinkumCoin.Wallet.Lambda.Models;

namespace DinkumCoin.Wallet.Lambda.Contracts
{
    public interface IMiningService
    {
        MiningResult AttemptMineNewCoin();
    }
}
