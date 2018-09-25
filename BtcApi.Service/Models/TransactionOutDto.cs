using System;

namespace BtcApi.Service.Models
{
    public struct TransactionOutDto
    {
        /// <summary>
        ///  дата поступления
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// адрес, на который был произведен перевод
        /// </summary>
        public string Address­ { get; set; }

        /// <summary>
        /// сумма переводав BTC.
        /// </summary>
        public decimal Amount­ { get; set; }

        /// <summary>
        /// количество подтверждений транзакции.
        /// </summary>
        public int Confirmations { get; set; }
    }
}
