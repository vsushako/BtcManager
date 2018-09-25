using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using BtcApi.Repository.Models;

namespace BtcApi.Repository.Repository
{
    /// <inheritdoc />
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected BtcContext Context { get; }

        public Repository(BtcContext context) => Context = context;

        /// <inheritdoc />
        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().AddOrUpdate(entity);
        }

        /// <inheritdoc />
        public async Task<TEntity> Get(Guid id)
        {
            return await Context.Set<TEntity>().FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await Context.Set<TEntity>().ToArrayAsync();
        }

        /// <inheritdoc />
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }
    }
}
