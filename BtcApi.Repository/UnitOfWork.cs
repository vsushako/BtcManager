using BtcApi.Repository.Repository;

namespace BtcApi.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly BtcContext _context;

        public IWalletRepository Wallets { get; }

        public ITransactionRepository Transactions { get; }

        public UnitOfWork() {
            _context = new BtcContext();
            Wallets = new WalletRepository(_context);
            Transactions = new TransactionRepository(_context);
        }

        public void Commit() => _context.SaveChanges();

        public void Dispose() => _context.Dispose(); 
    }
}
