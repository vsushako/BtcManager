using System.Threading.Tasks;

namespace Bitcoind
{
    /// <summary>
    /// Клас для вызова bitcoin сервера
    /// </summary>
    internal interface IBitcoindSender
    {
        /// <summary>
        /// Адрес сервера
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        string User { get; set; }

        /// <summary>
        /// Пароль
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Метод вызывает bitcoin сервер
        /// </summary>
        /// <param name="method">Метод</param>
        /// <param name="param">Параметры</param>
        /// <returns>Строка с результатом ответа</returns>
        Task<string> Send(string method, string[] param = null);
    }
}
