using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repository
{
    public class DbRepository<T> : IDbRepository<T> where T : class
    {

        private DbSet<T> entities;
        readonly IQueryable<T> dbSetIQueryable;
        string errorMessage = string.Empty;
        Type entityType;

        public DbRepository(DbSet<T> entities)
        {
            this.entities = entities ?? throw new ArgumentNullException("dbSet");
            dbSetIQueryable = entities as IQueryable<T>;
            entityType = typeof(T);
        }


        #region Contructor
        public Type ElementType
        {
            get { return dbSetIQueryable.ElementType; }
        }

        public Expression Expression
        {
            get { return dbSetIQueryable.Expression; }
        }

        public ObservableCollection<T> Local
        {
            get { return entities.Local; }
        }

        public IQueryProvider Provider
        {
            get { return dbSetIQueryable.Provider; }
        }
        #endregion

        #region Sync Method
        public IEnumerable<T> GetAll()
        {
            return entities.AsEnumerable();
        }

        public void Add(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Add(entity);
        }

        public void Update(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
        }

        public void Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }
            entities.Remove(entity);
        }

        public T Find(params object[] keyValues)
        {
            return entities.Find(keyValues);
        }

        public Task<T> FindAsync(params object[] keyValues)
        {
            return entities.FindAsync(keyValues);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return dbSetIQueryable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }


}
