using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Interfaces.Repository
{
    public interface IGenericRepository<T>
    {
        IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges);
        IQueryable<T> GetAll(bool trackChanges);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveChangesAsync();
    }
}
