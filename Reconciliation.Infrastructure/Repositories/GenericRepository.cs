using Microsoft.EntityFrameworkCore;
using Reconciliation.Application.Interfaces.Repository;
using Reconciliation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Infrastructure.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected ApplicationDbContext _applicationDbContext;

        public GenericRepository(ApplicationDbContext applicationDbContext)
        {
            _applicationDbContext = applicationDbContext;
        }

        public void Add(T entity)
        {
            _applicationDbContext.Set<T>().Add(entity);
        }

        public void Delete(T entity)
        {
            _applicationDbContext.Remove(entity);
        }

        public IQueryable<T> GetAll(bool trackChanges)
        {
            if (trackChanges)
            {
                return _applicationDbContext.Set<T>();
            }
            else
            {
                return _applicationDbContext.Set<T>().AsNoTracking();
            }
        }

        public IQueryable<T> GetByCondition(Expression<Func<T, bool>> expression, bool trackChanges)
        {
            if (trackChanges)
            {
                return _applicationDbContext.Set<T>().Where(expression);
            }
            else
            {
                return _applicationDbContext.Set<T>().Where<T>(expression).AsNoTracking();
            }
        }

        public async Task SaveChangesAsync()
        {
            await _applicationDbContext.SaveChangesAsync();
        }

        public void Update(T entity)
        {
            _applicationDbContext.Update(entity);
        }
    }
}
