using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BtcApi.Repository;
using BtcApi.Repository.Models;

namespace BtcApi.Service.Wallets
{
    internal class WalletsAccessManager : IWalletsAccessManager
    {
        public bool IsParallel { get; set; } = false;

        private Wallet _wallet;

        public Wallet GetWallet(IUnitOfWork unitOfWork, decimal amount)
        {
            IWalletLockManager currentLockedWallet;
            EventWaitHandle ewh;

            // Лочим переменную и ищем кошельки
            lock (LockedWallets.Wallets)
            {
                // Получаем кошельки из базы, если есть то сразу отдаем такой
                _wallet = Task.Run(() => unitOfWork.Wallets.GetFirstByAmountExcept(LockedWallets.Wallets.Keys, amount)).Result;
                if (_wallet != null)
                {
                    LockedWallets.Wallets.Add(_wallet.Id, new WalletLockManager { Wallet = _wallet, IsParallel = IsParallel });
                    LockedWallets.Wallets[_wallet.Id].Enqueue(amount);
                    return _wallet;
                }

                // Получаем кошелек с необходимым 
                currentLockedWallet = LockedWallets.Wallets.Values.Where(w => w.RestBalance >= amount).OrderBy(w => w.Count).FirstOrDefault();
                if (currentLockedWallet == null) throw new Exception("Недостаточно средств на кошельках");

                _wallet = currentLockedWallet.Wallet;
                ewh = currentLockedWallet.Enqueue(amount);
            }

            // Ставим в ожидание поток
            currentLockedWallet.Wait(ewh);
            // Если свободных кошельков нет, то добавляем в очередь
            return _wallet;
        }

        public void Dispose()
        {
            lock (LockedWallets.Wallets)
            {
                LockedWallets.Wallets[_wallet.Id].RunNext();
            }

        }
    }
}
