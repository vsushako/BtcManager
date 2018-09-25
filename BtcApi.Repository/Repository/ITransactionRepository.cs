using System.Collections.Generic;
using System.Threading.Tasks;
using BtcApi.Repository.Models;

namespace BtcApi.Repository.Repository
{
    public interface ITransactionRepository: IRepository<Transaction>
    {
        Task<IList<Transaction>> Get(IEnumerable<string> addresses);
        Task<IEnumerable<Transaction>> GetZeroConfirmations();
    }
}