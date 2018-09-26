using System;
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

        /// <summary>
        /// Получает кошельки с необходимым количеством денег за исключением определенного списка
        /// </summary>
        /// <param name="ids">список id которые не надо получать</param>
        /// <param name="amount">сумма</param>
        /// <returns></returns>
        Task<Wallet> GetFirstByAmountExcept(IEnumerable<Guid> ids, decimal amount);
    }
}