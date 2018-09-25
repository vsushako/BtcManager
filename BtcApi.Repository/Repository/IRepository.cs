using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BtcApi.Repository.Repository
{
    /// <summary>
    /// Взаимодействие с хранилищем
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// Получает сущность по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<TEntity> Get(Guid id);

        /// <summary>
        /// Получает все записи
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TEntity>> GetAll();

        /// <summary>
        /// Создает запись
        /// </summary>
        /// <param name="entity">Сущность</param>
        void Add(TEntity entity);

        /// <summary>
        /// Удаляет запись
        /// </summary>
        /// <param name="entity">Сущность</param>
        void Remove(TEntity entity);
    }
}
