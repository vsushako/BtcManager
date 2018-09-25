namespace Bitcoind.Models
{
    internal class ResultModel<TResult>
    {
        public TResult result { get; set; }
        public object error { get; set; }
        public string id { get; set; }
    }
}
