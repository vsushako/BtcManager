namespace Bitcoind.Models
{
    internal class ErrorModel
    {
        public object result { get; set; }
        public Error error { get; set; }
        public string id { get; set; }

        public class Error
        {
            public int code { get; set; }
            public string message { get; set; }
        }
    }
}