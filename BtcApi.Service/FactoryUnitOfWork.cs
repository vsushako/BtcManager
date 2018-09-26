using BtcApi.Repository;
using BtcApi.Service.Wallets;

namespace BtcApi.Service
{
    internal class WalletsAccessManagerFactory : IWalletsAccessManagerFactory
    {
        public IWalletsAccessManager GetWalletsAccessManager()
        {
            return new WalletsAccessManager();
        }
    }
}
