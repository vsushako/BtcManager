using BtcApi.Repository.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace BtcApi.Repository.Repository
{
    public class TransactionRepository : Repository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(BtcContext context) : base(context) { }

        public async Task<IList<Transaction>> Get(IEnumerable<string> addresses)
        {
            return await Context.Transactions.Where(t => addresses.Contains(t.Address)).ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetZeroConfirmations()
        {
            return await Context.Transactions.Where(t => t.Confirmations == 0 && t.Type != "payOut").ToListAsync();
        }
    }
}
