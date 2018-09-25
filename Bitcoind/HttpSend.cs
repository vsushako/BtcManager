using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Bitcoind.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bitcoind
{
    internal class HttpSend : IBitcoindSender
    {
        public string Address { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public async Task<string> Send(string method, string[] param = null)
        {
            var webRequest = (HttpWebRequest) WebRequest.Create(Address);
            webRequest.Credentials = new NetworkCredential(User, Password);
            webRequest.ContentType = "application/json-rpc";
            webRequest.Method = "POST";
            // Создаем объект с пропертями
            var joe = new JObject
            {
                new JProperty("jsonrpc", "1.0"),
                new JProperty("id", Guid.NewGuid()),
                new JProperty("method", method)
            };
            if (param?.Length > 0)
            {
                var props = new JArray();
                for (var i = 0; i < param.Count(); i++)
                    props.Add(param[i]);

                joe.Add(new JProperty("params", props));
            }
            else
            {
                joe.Add(new JProperty("params", new JArray()));
            }

            // Вызываем нужный нам метод
            var s = JsonConvert.SerializeObject(joe);
            var byteArray = Encoding.UTF8.GetBytes(s);
            webRequest.ContentLength = byteArray.Length;

            var dataStream = await webRequest.GetRequestStreamAsync();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();

            try
            {
                // Возвращаем ответ от сервера
                var webResponse = await webRequest.GetResponseAsync();

                using (var stream = webResponse.GetResponseStream())
                {
                    if (stream == null) return string.Empty;

                    using (var reader = new StreamReader(stream))
                        return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (var stream = e.Response.GetResponseStream())
                {
                    if (stream == null) throw;

                    using (var reader = new StreamReader(stream))
                    {
                        var error = JsonConvert.DeserializeObject<ErrorModel>(reader.ReadToEnd());
                        // Обертка над Exception для отправки сразу информации
                        throw new Exception(error?.error?.message, e);
                    }
                }
            }
        }
    }
}
