using System.Collections.Generic;
using System.Threading;
using BtcApi.Repository.Models;

namespace BtcApi.Service.Wallets
{
    public class WalletLockManager : IWalletLockManager
    {
        public bool IsParallel { get; set; }

        public decimal RestBalance { get; private set; }

        public int Count { get; private set; }

        private Wallet _wallet;
        public Wallet Wallet
        {
            get => _wallet;
            set
            {
                RestBalance = value.Balance;
                _wallet = value;
            }
        }

        private readonly Queue<EventWaitHandle> _handles = new Queue<EventWaitHandle>();

        public EventWaitHandle Enqueue(decimal amount)
        {
            Count++;
            RestBalance -= amount;
            if (Count == 1 || IsParallel) return null;

            var eventWaitHandle = new AutoResetEvent(false);
            _handles.Enqueue(eventWaitHandle);
            return eventWaitHandle;
        }

        public void Wait(EventWaitHandle eventWaitHandle)
        {
            eventWaitHandle?.WaitOne();
        }

        public void RunNext() {
            Count--;
            RestBalance = Wallet.Balance;

            if (_handles.Count == 0) return;

            var ewh = _handles.Dequeue();
            ewh.Set();
            ewh.Dispose();
        }
    }
}