using System;
using BtcApi.Repository;
using BtcApi.Repository.Repository;

namespace BtcApi.Tests
{
    public class FakeUnitOfWork : IUnitOfWork
    {
        public void Dispose()
        {
        }

        public void Commit()
        {   
        }

        public IWalletRepository Wallets { get; set; }
        public ITransactionRepository Transactions { get; set; }
    }
}