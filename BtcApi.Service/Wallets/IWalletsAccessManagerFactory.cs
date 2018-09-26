namespace BtcApi.Service.Wallets
{
    internal interface IWalletsAccessManagerFactory
    {
        IWalletsAccessManager GetWalletsAccessManager();
    }
}