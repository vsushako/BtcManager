using BtcApi.Service;
using BtcApi.Service.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BtcApi.Controllers
{
    public class BtcController : ApiController
    {
        public IBtc Btc { get; set; }

        public BtcController()
        {
            Btc = new Btc();
        }

        [HttpGet]
        public async Task<IEnumerable<TransactionOutDto>> GetLast()
        {
            try
            {
                return await Btc.GetLast();
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message));
            }
        }

        [HttpPost]
        public async Task<TransactionOutDto> SendBtc(TransactionInDto transaction)
         {
            try
            {
                return await Btc.SendBtc(transaction);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, e.Message));
            }
        }
    }
}
