


using ThinkTank.Application.Repository;

namespace ThinkTank.Application.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        public IGenericRepository<T> Repository<T>() where T : class;

        int Commit();
        Task<int> CommitAsync();
    }
}
