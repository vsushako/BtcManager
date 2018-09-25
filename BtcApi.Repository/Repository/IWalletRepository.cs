using System.Collections.Generic;
using System.Threading.Tasks;
using BtcApi.Repository.Models;

namespace BtcApi.Repository.Repository
{
    /// <summary>
    /// Репозиторий для кошельков
    /// </summary>
    public interface IWalletRepository: IRepository<Wallet>
    {
        /// <summary>
        /// Получает кошелек с необходимым количеством денег
        /// </summary>
        /// <param name="amount">количество</param>
        /// <returns></returns>
        Task<IList<Wallet>> GetByAmount(decimal amount);
    }
}