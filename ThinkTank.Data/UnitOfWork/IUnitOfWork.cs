
using ThinkTank.Data.Repository;

namespace ThinkTank.Data.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository<T> Repository<T>() where T : class;

        int Commit();
        Task<int> CommitAsync();
    }
}
