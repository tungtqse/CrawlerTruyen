using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public interface IDbRepository
    {
    }

    public interface IDbRepository<T> : IDbRepository, IQueryable<T>, IEnumerable<T>, IEnumerable, IQueryable where T : class
    {
        IEnumerable<T> GetAll();
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        T Find(params object[] keyValues);
        Task<T> FindAsync(params object[] keyValues);

    }
}
