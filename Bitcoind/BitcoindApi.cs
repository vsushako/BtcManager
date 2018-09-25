using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Bitcoind.Models;
using Newtonsoft.Json;

namespace Bitcoind
{
    public class BitcoindApi : IBitcoind
    {
        internal IBitcoindSender Sender { get; set; }

        public BitcoindApi()
        {
            Sender = new HttpSend
            {
                Address = ConfigurationManager.AppSettings["Address"],
                User = ConfigurationManager.AppSettings["User"],
                Password = ConfigurationManager.AppSettings["Password"],
            };
        }


        public async Task<decimal> GetBalance(string account)
        {
            var balance = await Sender.Send("getbalance", new[] { account });
            var result = JsonConvert.DeserializeObject<ResultModel<decimal>>(balance);
            return result.result;
        }

        public async Task<string> SendFrom(string account, string address, double amount)
        {
            var send = await Sender.Send("sendfrom", new []{ account, address, amount.ToString(CultureInfo.InvariantCulture) });
            var result = JsonConvert.DeserializeObject<ResultModel<string>>(send);
            return result.result;
        }

        public async Task<IList<ReceivedModel>> ListReceivedByAddress(int confirmations, bool empty, bool watch)
        {
            var blockchainInfo = await Sender.Send("listreceivedbyaddress");
            var result = JsonConvert.DeserializeObject<ResultModel<IList<ReceivedModel>>>(blockchainInfo);
            return result.result;
        }
    }
}
