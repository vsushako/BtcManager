using System;
using BtcApi.Repository.Models;
using BtcApi.Repository.Repository;

namespace BtcApi.Repository
{
    public interface IUnitOfWork: IDisposable
    {
        void Commit();
        IWalletRepository Wallets { get; }
        ITransactionRepository Transactions { get; }

    }
}
