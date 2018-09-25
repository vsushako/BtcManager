using System.Collections.Generic;
using BtcApi.Repository.Models;

namespace BtcApi.Service
{
    /// <summary>
    /// Класс для блокировки кошельков при транзакциях
    /// </summary>
    public static class LockedWallets
    {
        /// <summary>
        /// Залоченные кошельки
        /// </summary>
        public static IList<Wallet> Wallets { get; } = new List<Wallet>();

    }
}