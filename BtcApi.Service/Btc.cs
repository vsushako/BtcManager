using System;
using BtcApi.Service.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bitcoind;
using BtcApi.Repository;
using BtcApi.Repository.Models;
using BtcApi.Service.Wallets;

namespace BtcApi.Service
{
    public class Btc: IBtc
    {
        internal IBitcoind BitcoindApi { get; set; }
        internal IUnitOfWorkFactory UnitOfWorkFactory { get; set; }
        internal IWalletsAccessManagerFactory WalletsAccessManagerFactory { get; set; }

        public Btc()
        {
            BitcoindApi = new BitcoindApi();
            UnitOfWorkFactory = new UnitOfWorkFactory();
            WalletsAccessManagerFactory = new WalletsAccessManagerFactory();
        }

        /// <summary>
        /// Отправка btc 
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<TransactionOutDto> SendBtc(TransactionInDto transaction)
        {
            var date = DateTime.Now;
            // Создаем транзакцию
            using (var unitOfWork = UnitOfWorkFactory.GetUnitOfWork())
            {
                // Получаем кошелек
                using (var walletManager = WalletsAccessManagerFactory.GetWalletsAccessManager())
                {
                    var wallet = walletManager.GetWallet(unitOfWork, transaction.Amount);
                    // Отправляем btc
                    var result = await BitcoindApi.SendFrom(wallet.Account, transaction.Address, transaction.Amount);
                    unitOfWork.Transactions.Add(new Transaction
                    {
                        Id = Guid.NewGuid(),
                        Confirmations = 0,
                        Address = transaction.Address,
                        Account = wallet.Account,
                        Amount = transaction.Amount,
                        Date = date,
                        TransactionNumber = result,
                        Type = "payOut"
                    });

                    // Если все ок, то уменьшаем баланс
                    wallet.Balance = await BitcoindApi.GetBalance(wallet.Account);
                    unitOfWork.Wallets.Add(wallet);
                    unitOfWork.Commit();
                }
            }

            return new TransactionOutDto { Confirmations = 0, Address­ = transaction.Address, Amount­ = transaction.Amount, Date = date };
        }

        /// <summary>
        /// Получение последних транзакций на ввод
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<TransactionOutDto>> GetLast()
        {
            using (var unitOfWork = UnitOfWorkFactory.GetUnitOfWork())
            {
                var bitcoindTransactions = await BitcoindApi.ListReceivedByAddress(6, true, true);
                var result = new List<TransactionOutDto>();

                var transactions = (await unitOfWork.Transactions.Get(bitcoindTransactions.Select(bt => bt.address)));
                foreach (var transaction in bitcoindTransactions)
                {
                    var dbTransaction = transactions.FirstOrDefault(t => t.Address == transaction.address && t.Account == transaction.account &&
                        transaction.amount == t.Amount);

                    if (dbTransaction != null)
                        // обновляем количество подтверждений
                        dbTransaction.Confirmations = transaction.confirmations ?? 0;

                    // Если транзакций меньше 3 то надо такие получать
                    if (transaction.confirmations < 3)
                        result.Add(new TransactionOutDto
                        {
                            Confirmations = transaction.confirmations ?? 0,
                            Address­ = transaction.address,
                            Amount­ = transaction.amount,
                            Date = dbTransaction?.Date
                        });
                }

                // Получаем транзакции которые еще не получались данным методом
                var zeroTransactions = (await unitOfWork.Transactions.GetZeroConfirmations())?.ToList();
                if (zeroTransactions?.Count > 0)
                {
                    foreach (var transaction in zeroTransactions)
                        result.Add(new TransactionOutDto
                        {
                            Confirmations = transaction.Confirmations,
                            Address­ = transaction.Address,
                            Amount­ = transaction.Amount,
                            Date = transaction.Date
                        });
                }

                unitOfWork.Commit();

                return result;
            }
        }
    }
}
