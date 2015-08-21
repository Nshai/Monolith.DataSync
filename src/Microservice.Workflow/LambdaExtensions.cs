using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microservice.Workflow
{
    public static class LambdaExtensions
    {
        public static string GetPropertyName<TModel, TProperty>(this TModel model, Expression<Func<TModel, TProperty>> property)
        {
            if (property.Body is MemberExpression)
                return ((MemberExpression)property.Body).Member.Name;

            if (property.Body is UnaryExpression)
                return ((MemberExpression)((UnaryExpression)property.Body).Operand).Member.Name;

            throw new ArgumentException("Expression must represent field or property access.");
        }

        public static void SetPropertyValue<T>(this T target, Expression<Func<T, object>> memberLamda, object value)
        {
            var memberSelectorExpression = memberLamda.Body as MemberExpression;
            var unaryExpression = memberLamda.Body as UnaryExpression;

            if (memberSelectorExpression == null)
            {
                if (unaryExpression != null)
                {
                    memberSelectorExpression = unaryExpression.Operand as MemberExpression;
                }
                else
                {
                    return;
                }
            }

            var property = memberSelectorExpression.Member as PropertyInfo;
            if (property != null)
            {
                property.SetValue(target, value, null);
            }
        }
    }
}
