

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
