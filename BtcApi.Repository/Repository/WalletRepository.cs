using System.Collections.Generic;
using BtcApi.Repository.Models;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BtcApi.Repository.Repository
{
    public class WalletRepository : Repository<Wallet>, IWalletRepository
    {
        public WalletRepository(BtcContext context) : base(context) {  }
        
        public async Task<IList<Wallet>> GetByAmount(decimal amount)
        {
            return await Context.Wallets.Where(w => w.Balance > amount).ToListAsync();
        }
    }
}
