using System;
using BtcApi.Repository;
using BtcApi.Repository.Models;

namespace BtcApi.Service.Wallets
{
    /// <summary>
    /// Менеджер доступа к кошелькам
    /// </summary>
    internal interface IWalletsAccessManager: IDisposable
    {
        /// <summary>
        /// Флаг запускать ли потоки параллельно
        /// </summary>
        bool IsParallel { get; set; }

        /// <summary>
        /// Получение кошелька
        /// </summary>
        /// <param name="unitOfWork">Транзакция</param>
        /// <param name="amount">Сумма</param>
        /// <returns></returns>
        Wallet GetWallet(IUnitOfWork unitOfWork, decimal amount);
    }
}