/*
 * Copyright 2008-2014 Matthew Cosand
 */
namespace Kcsara.Database.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static class KcsarExtensions
    {
        public static IQueryable<TSource> WhereIn<TSource, TKey>(
                this IQueryable<TSource> source1,
                Expression<Func<TSource, TKey>> keySelector,
                IEnumerable<TKey> source2)
        {
            if (null == source1)
                throw new ArgumentNullException("source1");
            if (null == keySelector)
                throw new ArgumentNullException("keySelector");
            if (null == source2)
                throw new ArgumentNullException("source2");
            Expression where = null;
            foreach (TKey value in source2)
            {
                Expression equal = Expression.Equal(
                            keySelector.Body,
                            Expression.Constant(value, typeof(TKey))
                            );
                if (null == where)
                    where = equal;
                else
                    where = Expression.OrElse(where, equal);
            }
            return source1.Where<TSource>(
                Expression.Lambda<Func<TSource, bool>>(
                    where, keySelector.Parameters));
        }
    }
}
