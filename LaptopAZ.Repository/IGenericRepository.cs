using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LaptopAZ.Repository
{
    public interface IGenericRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> Find(Expression<Func<T, bool>> predicate);
        T GetById(object id);
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
        int Count(Expression<Func<T, bool>> predicate = null);
        bool Any(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query();
    }
}
