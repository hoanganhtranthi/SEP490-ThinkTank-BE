
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;

namespace ThinkTank.Data.Repository
{

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private static ThinkTankContext Context;
        private static DbSet<T> Table { get; set; }
        public GenericRepository(ThinkTankContext context)
        {
            Context = context;
            Table = Context.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
            await Context.AddAsync(entity);
        }

        public async Task RemoveAsync(T entity)
        {
            Context.Remove(entity);
        }

        public async Task<List<T>> GetWhere(Expression<Func<T, bool>>? filter = null)
        {
            IQueryable<T> query = Table;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>>? filter = null)
        {
             IQueryable<T> query = Table;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }


        public EntityEntry<T> Delete(T entity)
        {
            return Context.Remove(entity);
        }

        public IQueryable<T> FindAll(Func<T, bool> predicate)
        {
            return Table.Where(predicate).AsQueryable();
        }

        public T Find(Func<T, bool> predicate)
        {
            return Table.FirstOrDefault(predicate);
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await Table.SingleOrDefaultAsync(predicate);
        }

        public async Task<T> GetById(int id)
        {
            return await Table.FindAsync(id);
        }
        public async Task Update(T entity, int Id)
        {
            var existEntity = await GetById(Id);
            Context.Entry(existEntity).CurrentValues.SetValues(entity);
            Table.Update(existEntity);
        }

        public DbSet<T> GetAll()
        {
            return Table;
        }

        public void DeleteRange(T[] entity)
        {
            Context.RemoveRange(entity);
        }
    }
    
    
}
