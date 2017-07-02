using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Ap.Infrastructure
{
    public static class QueryableExtensions
    {
        private readonly static Lazy<MethodInfo> StartWithMethodLazy = new Lazy<MethodInfo>(() =>
        {
            var m = typeof(string).GetMethod("StartsWith", new[] { typeof(string) });
            return m;

        }, true);

        public static IQueryable<TElement> WhereOrLike<TElement>(this IQueryable<TElement> query
            , Expression<Func<TElement, string>> valueSelector
            , IEnumerable<string> values)
        {
            if (values==null ||!values.Any())
            {
                return query;
            }

            var startsWithMethod = StartWithMethodLazy.Value;

            var exprs = values.Select(rocg => (Expression)Expression.Call(valueSelector.Body, startsWithMethod, Expression.Constant(rocg, typeof(string)))).ToArray()
                .Aggregate<Expression>((prev, next) => Expression.Or(prev, next));
            var condition = Expression.Lambda<Func<TElement, bool>>(exprs, valueSelector.Parameters);
            return query.Where(condition);
        }

        private readonly static Lazy<MethodInfo> ContainsMethodLazy = new Lazy<MethodInfo>(() =>
        {
            var m = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            return m;

        }, true);
        public static IQueryable<TElement> WhereOrContains<TElement>(this IQueryable<TElement> query
            , Expression<Func<TElement, string>> valueSelector
            , IEnumerable<string> values)
        {
            if (values==null ||!values.Any())
            {
                return query;
            }

            var containsMethod = ContainsMethodLazy.Value;

            var exprs = values.Select(rocg => (Expression)Expression.Call(valueSelector.Body, containsMethod, Expression.Constant(rocg, typeof(string))))
                .Aggregate<Expression>((prev, next) => Expression.Or(prev, next));
            var condition = Expression.Lambda<Func<TElement, bool>>(exprs, valueSelector.Parameters);
            return query.Where(condition);
        }
        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> query
            , Expression<Func<TElement, TValue>> valueSelector
            , IEnumerable<TValue> values)
        {
            if (values==null || !values.Any())
            {
                return query;
            }

            var body = valueSelector.Body;
            var valueType = typeof(TValue);
            var exprs = values.Select(_ => (Expression)Expression.Equal(body, Expression.Constant(_, valueType)))
                .Aggregate<Expression>((prev, next) => Expression.Or(prev, next));
            var condition = Expression.Lambda<Func<TElement, bool>>(exprs, valueSelector.Parameters);
            return query.Where(condition);
        }

        public static IQueryable<TElement> WhereIfNotEmpty<TElement>(this IQueryable<TElement> query
            , Expression<Func<TElement, bool>> predicate
            , string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return query;
            }
            return query.Where(predicate);
        }

        public static IQueryable<TElement> WhereIfNotEmpty<TElement, TValue>(this IQueryable<TElement> query
            , Expression<Func<TElement, bool>> predicate
            , Nullable<TValue> value) where TValue : struct
        {
            if (!value.HasValue)
            {
                return query;
            }
            return query.Where(predicate);
        }
    }
}
