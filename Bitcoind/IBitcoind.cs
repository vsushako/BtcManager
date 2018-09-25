using System.Collections.Generic;
using System.Threading.Tasks;
using Bitcoind.Models;

namespace Bitcoind
{
    /// <summary>
    /// Класс для работы с Bitcoind
    /// </summary>
    public interface IBitcoind
    {
        /// <summary>
        /// Получение баланса
        /// </summary>
        /// <returns></returns>
        Task<decimal> GetBalance(string account);

        /// <summary>
        /// Получение количества подтверждений
        /// </summary>
        /// <param name="confirmations">Количество подтверждений</param>
        /// <param name="empty">Включать ли пустые</param>
        /// <param name="watch">Включать ли watch-only</param>
        /// <returns></returns>
        Task<IList<ReceivedModel>> ListReceivedByAddress(int confirmations, bool empty, bool watch);

        /// <summary>
        /// Отправка биткоинов
        /// </summary>
        /// <param name="account">аккаунт с которого отправляем</param>
        /// <param name="address">адрес куда посылать</param>
        /// <param name="amount">Количсетво</param>
        /// <returns></returns>
        Task<string> SendFrom(string account, string address, double amount);
    }
}