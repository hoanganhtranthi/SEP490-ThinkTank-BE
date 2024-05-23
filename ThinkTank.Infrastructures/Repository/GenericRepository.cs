
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;
using ThinkTank.Application.Repository;
using ThinkTank.Infrastructures.DatabaseContext;

namespace ThinkTank.Infrastructures.Repository
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
        public async Task UpdateDispose(T entity, int Id)
        {
            using (var context = new ThinkTankContext())
            {
                T existing = context.Set<T>().Find(Id);
                if (existing != null)
                {
                    context.Entry(existing).CurrentValues.SetValues(entity);
                    context.Set<T>().Update(existing);
                }
            }
        }
        public async Task<List<T>> GetAllAsync(params Expression<Func<T, object>>[] includes) =>
                await includes
                .Aggregate(Table.AsQueryable(),
                     (entity, property) => entity.Include(property).IgnoreAutoIncludes())
                .ToListAsync();

        public async Task<T?> GetByAsync(Expression<Func<T, bool>>? filter = null, params Expression<Func<T, object>>[] includes)
        {
            return await includes
               .Aggregate(Table.AsQueryable(),
                   (entity, property) => entity.Include(property))
               .AsNoTracking().Where(filter)
               .FirstOrDefaultAsync();
        }
        public DbSet<T> GetAll()
        {
            return Table;
        }

        public async Task DeleteRange(T[] entity)
        {
            Context.RemoveRange(entity);
        }
    }
    
    
}
