﻿using System;
using BtcApi.Service.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bitcoind;
using BtcApi.Repository;
using BtcApi.Repository.Models;

namespace BtcApi.Service
{
    public class Btc: IBtc
    {
        internal IBitcoind BitcoindApi { get; set; }
        internal IUnitOfWorkFactory UnitOfWorkFactory { get; set; }

        public Btc()
        {
            BitcoindApi = new BitcoindApi();
            UnitOfWorkFactory = new UnitOfWorkFactory();
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
                Wallet wallet = null;
                do
                {
                // Лочим переменную и ищем кошельки
                    lock (LockedWallets.Wallets)
                    {
                        // Получаем подходящий кошелек
                        var wallets = Task.Run(() => unitOfWork.Wallets.GetByAmount(transaction.Amount)).Result;
                        if (wallets == null || !wallets.Any()) throw new Exception("Недостаточно средств на кошельках");

                        foreach (var w in wallets)
                            if (!LockedWallets.Wallets.Contains(w))
                            {
                                wallet = w;
                                LockedWallets.Wallets.Add(wallet);
                                break;
                            }
                    }

                    if(wallet == null)
                            Thread.Sleep(100);

                } while (wallet == null);
                
                try
                {
                    // Отправляем 
                    var result = await BitcoindApi.SendFrom(wallet.Account, transaction.Address, transaction.Amount);
                    unitOfWork.Transactions.Add(new Transaction
                    {
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
                finally 
                {
                    lock (LockedWallets.Wallets)
                    {
                        LockedWallets.Wallets.Remove(wallet);
                    }
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