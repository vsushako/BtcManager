using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bitcoind.Models
{
    public struct ReceivedModel
    {
        public string address { get; set; }
        public string account { get; set; }
        public decimal amount { get; set; }
        public int? confirmations { get; set; }
        public string label { get; set; }
    }
}
