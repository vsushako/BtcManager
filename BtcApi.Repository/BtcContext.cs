using System.Data.Entity;
using BtcApi.Repository.Models;

namespace BtcApi.Repository
{
    public class BtcContext: DbContext
    {
        public BtcContext()
            : base("name=BtcContext")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public virtual DbSet<Wallet> Wallets { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<BtcContext>(null);
            base.OnModelCreating(modelBuilder);
        }
    }
}
