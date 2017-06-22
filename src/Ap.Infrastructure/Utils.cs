using System;
using System.Linq.Expressions;

namespace Ap.Infrastructure
{
    public static class Utils
    {
        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expr)
        {
            var e = expr.Body;
            if (e.NodeType == ExpressionType.MemberAccess)
            {
                return (e as MemberExpression).Member.Name;
            }
            throw new InvalidOperationException($"expression must be MemberAccess type. But given is {e.NodeType}");
        }

        public static string GetPropertyName<T>(Expression<Func<T, object>> expr)
        {
            var e = expr.Body;
            if (e.NodeType == ExpressionType.Convert)
            {
                e = (e as UnaryExpression).Operand;
            }
            if (e.NodeType == ExpressionType.MemberAccess)
            {
                return (e as MemberExpression).Member.Name;
            }
            throw new InvalidOperationException($"expression must be MemberAccess type. But given is {e.NodeType}");
        }

        public static T SetupUpdateInfo<T>(this T t, string @operator) where T : IHavingUpdateInfo
        {
            t.Update.Operator = @operator;
            t.Update.TimeInUtc = GetUtcNow();
            return t;
        }

        public static T SetupCreationInfo<T>(this T t, string @operator) where T : IHavingCreationInfo
        {
            t.Creation.Operator = @operator;
            t.Creation.TimeInUtc = GetUtcNow();
            return t;
        }

        public static DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        public static string ToStsLongString(this DateTime? dateTime)
        {
            if(dateTime.HasValue)
            {
                return dateTime.Value.ToString(ApLongDateFormat);
            }
            return string.Empty;
        }

        /// <summary>
        /// yyyy-MM-ddTHH:mm:ss+00:00
        /// </summary>
        public static readonly string ApLongDateFormat="yyyy-MM-ddTHH:mm:ss+00:00"; //ISO date format reference to https://www.w3.org/TR/NOTE-datetime

        /// <summary>
        /// yyyy-MM-dd
        /// </summary>
        public static readonly string ApShortDateFormat="yyyy-MM-dd";
    }
}
