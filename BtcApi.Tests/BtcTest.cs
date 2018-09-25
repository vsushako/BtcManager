using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Bitcoind;
using BtcApi.Repository;
using BtcApi.Repository.Models;
using BtcApi.Repository.Repository;
using BtcApi.Service;
using BtcApi.Service.Models;
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
            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetByAmount(It.IsAny<decimal>())).ReturnsAsync(new List<Wallet> {new Wallet { Account = "account"}});

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
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()))
                .ReturnsAsync((string account, string address, double amount) =>
                {
                    Assert.AreEqual(address, "address");
                    Assert.AreEqual(amount, 1);
                    return "transaction";
                });

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

            var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
            var result = await btc.SendBtc(new TransactionInDto { Amount = 1, Address = "address" });

            bitcoindApiMock.Verify(m => m.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()), Times.Once);
            Assert.AreEqual(result.Amount­, 1);
        }

        [TestMethod]
        public async Task TestSendBtc_TestLock_ExpectsOk()
        {
            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetByAmount(It.IsAny<decimal>())).ReturnsAsync(new List<Wallet> { new Wallet { Account = "account" } });

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
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()))
                .ReturnsAsync((string account, string address, double amount) =>
                {
                    Assert.AreEqual(address, "address");
                    Assert.AreEqual(amount, 1);
                    return "transaction";
                });

            var unitOfWorkFactory = new Mock<IUnitOfWorkFactory> { CallBase = false };
            unitOfWorkFactory.Setup(factory => factory.GetUnitOfWork()).Returns(unitOfWork.Object);

            var btc = new Btc { BitcoindApi = bitcoindApiMock.Object, UnitOfWorkFactory = unitOfWorkFactory.Object };
            var result = await btc.SendBtc(new TransactionInDto { Amount = 1, Address = "address" });

            Assert.AreEqual(LockedWallets.Wallets.Count, 0);

            bitcoindApiMock.Verify(m => m.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()), Times.Once);
            Assert.AreEqual(result.Amount­, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task TestSendBtc_TestLockWithException_ExpectsOk()
        {
            var wallet = new Mock<IWalletRepository> {CallBase = false};
            wallet.Setup(w => w.GetByAmount(It.IsAny<decimal>()))
                .ReturnsAsync(new List<Wallet> {new Wallet {Account = "account"}});

            var unitOfWork = new Mock<IUnitOfWork> {CallBase = false};
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);

            var bitcoindApiMock = new Mock<IBitcoind> {CallBase = false};
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>())).Callback(
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
                Assert.AreEqual(LockedWallets.Wallets.Count, 0);
            }
        }

        [TestMethod]
        public void TestSendBtc_TestLockThreads_ExpectsOk()
        {
            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetByAmount(It.IsAny<decimal>())).ReturnsAsync(() => new List<Wallet> {new Wallet {Account = "account"}});

            var transaction = new Mock<ITransactionRepository> { CallBase = false };
            transaction.Setup(t => t.Add(It.IsAny<Transaction>())).Callback(() => { Assert.AreEqual(LockedWallets.Wallets.Count, 1); });

            var unitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);
            unitOfWork.Setup(u => u.Transactions).Returns(transaction.Object);

            var bitcoindApiMock = new Mock<IBitcoind> { CallBase = false };
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>())).Callback(() => {
                // ДОбавляем сон что бы была задержка
                Thread.Sleep(100);
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

            // Проверяем что мы несколько раз вызывали метод получения, соответственно ждали очереди
            wallet.Verify(w => w.GetByAmount(It.IsAny<decimal>()), Times.AtLeast(3));
        }

        [TestMethod]
        public void TestSendBtc_TestNoWait_ExpectsOk()
        {
            var wallet = new Mock<IWalletRepository> { CallBase = false };
            wallet.Setup(w => w.GetByAmount(It.IsAny<decimal>())).ReturnsAsync(() => new List<Wallet> { new Wallet { Account = "account" }, new Wallet { Account = "account1" } });

            var transaction = new Mock<ITransactionRepository> { CallBase = false };
            transaction.Setup(t => t.Add(It.IsAny<Transaction>())).Callback(() => { Assert.AreEqual(LockedWallets.Wallets.Count, 1); });

            var unitOfWork = new Mock<IUnitOfWork> { CallBase = false };
            unitOfWork.Setup(u => u.Wallets).Returns(wallet.Object);
            unitOfWork.Setup(u => u.Transactions).Returns(transaction.Object);

            var bitcoindApiMock = new Mock<IBitcoind> { CallBase = false };
            bitcoindApiMock.Setup(b => b.SendFrom(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>()))
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

            wallet.Verify(w => w.GetByAmount(It.IsAny<decimal>()), Times.AtLeast(2));
        }
    }
}
