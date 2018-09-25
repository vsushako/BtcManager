using System.Collections.Generic;
using System.Threading.Tasks;
using BtcApi.Service.Models;

namespace BtcApi.Service
{
    /// <summary>
    /// Бизнес логика для работы с кошельками
    /// </summary>
    public interface IBtc
    {
        /// <summary>
        /// Получает последние поступления
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TransactionOutDto>> GetLast();

        /// <summary>
        /// Отправка на адрес пользователя
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<TransactionOutDto> SendBtc(TransactionInDto transaction);
    }
}