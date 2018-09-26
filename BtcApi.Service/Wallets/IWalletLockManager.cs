using System.Threading;
using BtcApi.Repository.Models;

namespace BtcApi.Service.Wallets
{
    /// <summary>
    /// Менеджер очереди блокировок для кошельков
    /// </summary>
    public interface IWalletLockManager
    {
        /// <summary>
        /// Количсетво инстансов 
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Флаг запускать ли инстансы параллельно
        /// </summary>
        bool IsParallel { get; set; }

        /// <summary>
        /// Остаток баланса после запуска всех инстансов
        /// </summary>
        decimal RestBalance { get; }

        /// <summary>
        /// Кошелек
        /// </summary>
        Wallet Wallet { get; set; }

        /// <summary>
        /// Запуск ожидающих потоков
        /// </summary>
        void RunNext();

        /// <summary>
        /// Команда ожидания 
        /// </summary>
        void Wait(EventWaitHandle eventWaitHandle);

        /// <summary>
        /// Добавление потоков
        /// </summary>
        /// <param name="amount">Сумма которую потребует поток</param>
        EventWaitHandle Enqueue(decimal amount);
    }
}