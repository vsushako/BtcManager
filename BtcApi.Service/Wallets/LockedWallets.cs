using System;
using System.Collections.Generic;

namespace BtcApi.Service.Wallets
{
    /// <summary>
    /// Класс для блокировки кошельков при транзакциях
    /// </summary>
    public static class LockedWallets
    {
        /// <summary>
        /// Залоченные кошельки
        /// </summary>
        public static IDictionary<Guid, IWalletLockManager> Wallets { get; } = new Dictionary<Guid, IWalletLockManager>();

    }
}