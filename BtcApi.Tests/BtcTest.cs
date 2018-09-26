using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bitcoind;
using BtcApi.Repository;
using BtcApi.Repository.Models;
using BtcApi.Repository.Repository;
using BtcApi.Service;
using BtcApi.Service.Models;
using BtcApi.Service.Wallets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace BtcApi.Tests
{
    [TestClass]
    public class BtcTest
    {
        [TestMethod]
        public async Task TestSendBtc_TestSave_ExpectsOk()
        {
            LockedWallets.Wallets.Clear();

            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetFirstByAmountExcept(It.IsAny<IEnumerable<Guid>>(), It.IsAny<decimal>())).ReturnsAsync(new Wallet { Account = "account"});

            var transaction = new Mock<ITransactionRepository> { CallBase = false };
            transaction.Setup(t => t.Add(It.IsAny<Transaction>())).Callback<Transaction>(t =>
            {
                Assert.AreEqual(t.Address, "address");
                Assert.AreEqual(t.Account, "account");
                Assert.AreEqual(t.Amount, 1);
            });


            var unitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);
            unitOfWork.Setup(u => u.Transactions).Returns(transaction.Object);

            var bitcoindApiMock = new Mock<IBitcoind> {CallBase = false};
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync((string account, string address, decimal amount) =>
                {
                    Assert.AreEqual(address, "address");
                    Assert.AreEqual(amount, 1);
                    return "transaction";
                });

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

            var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
            var result = await btc.SendBtc(new TransactionInDto { Amount = 1, Address = "address" });

            bitcoindApiMock.Verify(m => m.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
            Assert.AreEqual(result.Amount­, 1);
        }

        [TestMethod]
        public async Task TestSendBtc_TestLock_ExpectsOk()
        {
            LockedWallets.Wallets.Clear();

            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetFirstByAmountExcept(It.IsAny<IEnumerable<Guid>>(), It.IsAny<decimal>())).ReturnsAsync(new Wallet { Account = "account" });

            var transaction = new Mock<ITransactionRepository> { CallBase = false };
            transaction.Setup(t => t.Add(It.IsAny<Transaction>())).Callback<Transaction>(t =>
            {
                Assert.AreEqual(LockedWallets.Wallets.Count, 1);
                Assert.AreEqual(t.Address, "address");
                Assert.AreEqual(t.Account, "account");
                Assert.AreEqual(t.Amount, 1);
            });

            var unitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);
            unitOfWork.Setup(u => u.Transactions).Returns(transaction.Object);

            var bitcoindApiMock = new Mock<IBitcoind> { CallBase = false };
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()))
                .ReturnsAsync((string account, string address, decimal amount) =>
                {
                    Assert.AreEqual(address, "address");
                    Assert.AreEqual(amount, 1);
                    return "transaction";
                });

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

            var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
            var result = await btc.SendBtc(new TransactionInDto { Amount = 1, Address = "address" });

            Assert.AreEqual(LockedWallets.Wallets.Count, 1);
            Assert.AreEqual(LockedWallets.Wallets.First().Value.Count, 0);

            bitcoindApiMock.Verify(m => m.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Once);
            Assert.AreEqual(result.Amount­, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestSendBtc_TestLockWithException_ExpectsOk()
        {
            LockedWallets.Wallets.Clear();
            var wallet = new Mock<IWalletRepository> {CallBase = false};
            wallet.Setup(w => w.GetFirstByAmountExcept(It.IsAny<IEnumerable<Guid>>(), It.IsAny<decimal>())).ReturnsAsync(new Wallet { Account = "account" });


            var unitOfWork = new Mock<IUnitOfWork> {CallBase = false};
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);

            var bitcoindApiMock = new Mock<IBitcoind> {CallBase = false};
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Callback(
                    () => { Assert.AreEqual(LockedWallets.Wallets.Count, 1); })
                .ThrowsAsync(new Exception());

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> {CallBase = false};
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

            var btc = new Btc {BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object};
            try
            {
                await btc.SendBtc(new TransactionInDto {Amount = 1, Address = "address"});
            }
            finally
            {
                Assert.AreEqual(LockedWallets.Wallets.Count, 1);
                Assert.AreEqual(LockedWallets.Wallets.First().Value.Count, 0);
            }
        }

        [TestMethod]
        public void TestSendBtc_TestLockThreads_ExpectsOk()
        {
            LockedWallets.Wallets.Clear();
            var suspendedThreads = 1;

            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetFirstByAmountExcept(It.IsAny<IEnumerable<Guid>>(), It.IsAny<decimal>())).ReturnsAsync(new Wallet { Account = "account" });

            var transaction = new Mock<ITransactionRepository> { CallBase = false };
            transaction.Setup(t => t.Add(It.IsAny<Transaction>())).Callback(() => { Assert.AreEqual(LockedWallets.Wallets.Count, 1); });

            var unitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);
            unitOfWork.Setup(u => u.Transactions).Returns(transaction.Object);

            var bitcoindApiMock = new Mock<IBitcoind> { CallBase = false };
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Callback(() => {
                Assert.AreEqual(suspendedThreads, LockedWallets.Wallets.First().Value.Count);
                suspendedThreads--;
            }).ReturnsAsync("transaction");

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

  
            var t1 = new Thread(async () =>
            {
                var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
                await btc.SendBtc(new TransactionInDto {Amount = 1, Address = "address"});
            });
            t1.Start();
            var t2 = new Thread(async () =>
            {
                var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
                await btc.SendBtc(new TransactionInDto {Amount = 1, Address = "address"});
            });
            t2.Start();

            t1.Join();
            t2.Join();
        }

        [TestMethod]
        public void TestSendBtc_TestNoWait_ExpectsOk()
        {
            LockedWallets.Wallets.Clear();

            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetFirstByAmountExcept(It.IsAny<IEnumerable<Guid>>(), It.IsAny<decimal>())).ReturnsAsync(new Wallet { Account = "account" + new Random().Next() });

            var transaction = new Mock<ITransactionRepository> { CallBase = false };
            transaction.Setup(t => t.Add(It.IsAny<Transaction>())).Callback(() => { Assert.AreEqual(LockedWallets.Wallets.Count, 1); });

            var unitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);
            unitOfWork.Setup(u => u.Transactions).Returns(transaction.Object);

            var bitcoindApiMock = new Mock<IBitcoind> { CallBase = false };
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<decimal>())).Callback(() => {
                    // Проверяем что у нас не копятся очереди
                    Assert.AreEqual(2, LockedWallets.Wallets.Count);
                    Assert.AreEqual(0, LockedWallets.Wallets.First().Value.Count);
                    Assert.AreEqual(0, LockedWallets.Wallets.Last().Value.Count);
            })
                .ReturnsAsync("transaction");

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

            var t1 = new Thread(async () =>
            {
                var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
                await btc.SendBtc(new TransactionInDto { Amount = 1, Address = "address" });
            });
            t1.Start();
            var t2 = new Thread(async () =>
            {
                var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
                await btc.SendBtc(new TransactionInDto { Amount = 1, Address = "address" });
            });
            t2.Start();

            t1.Join();
            t2.Join();

            wallet.Verify(w => w.GetFirstByAmountExcept(It.IsAny<IEnumerable<Guid>>(), It.IsAny<decimal>()), Times.Exactly(2));
        }
    }
}
