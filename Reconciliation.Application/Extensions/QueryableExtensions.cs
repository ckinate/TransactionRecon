using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reconciliation.Application.Extensions
{
    public static class QueryableExtensions
    {
        // Generic filtering method that can work with any entity type
        public static IQueryable<TEntity> ApplyFilters<TEntity, TInput>(
            this IQueryable<TEntity> query,
            TInput input,
            Func<TInput, IEnumerable<Expression<Func<TEntity, bool>>>> filterExpressionBuilder)
            where TEntity : class
        {
            if (input == null || filterExpressionBuilder == null)
                return query;

            var filterExpressions = filterExpressionBuilder(input);

            foreach (var filterExpression in filterExpressions)
            {
                if (filterExpression != null)
                {
                    query = query.Where(filterExpression);
                }
            }

            return query;
        }
        // Generic sorting method that can work with any entity type
        public static IQueryable<TEntity> ApplySorting<TEntity, TInput, TKey>(
            this IQueryable<TEntity> query,
            TInput input,
            Expression<Func<TEntity, TKey>> sortKeySelector,
            Func<TInput, bool> isAscendingFunc)
            where TEntity : class
        {
            if (input == null || sortKeySelector == null || isAscendingFunc == null)
                return query;

            if (isAscendingFunc(input))
            {
                return query.OrderBy(sortKeySelector);
            }
            else
            {
                return query.OrderByDescending(sortKeySelector);
            }
        }

        // This overload allows for dynamic sort key selection based on input
        public static IQueryable<TEntity> ApplySorting<TEntity, TInput>(
            this IQueryable<TEntity> query,
            TInput input,
            Func<TInput, Expression<Func<TEntity, object>>> sortKeySelectorFunc,
            Func<TInput, bool> isAscendingFunc)
            where TEntity : class
        {
            if (input == null || sortKeySelectorFunc == null || isAscendingFunc == null)
                return query;

            var sortKeySelector = sortKeySelectorFunc(input);

            if (sortKeySelector == null)
                return query;

            if (isAscendingFunc(input))
            {
                return query.OrderBy(sortKeySelector);
            }
            else
            {
                return query.OrderByDescending(sortKeySelector);
            }
        }
    }
}
