using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Microservice.Workflow.Domain
{
    public static class ODataBuilder
    {
        /// <summary>
        /// Builds the odata filter with 'Eq' odata operator
        /// </summary>
        /// <typeparam name="T">Type of event</typeparam>
        /// <typeparam name="TP">Type of Property in the Event Class</typeparam>
        /// <param name="property">Property on the Event Class</param>
        /// <param name="value">Value for the property</param>
        /// <returns></returns>
        public static FilterCondition BuildFilterForProperty<T, TP>(Expression<Func<T, TP>> property, TP value)
            where T : new()
        {
            var @event = new T();
            return new FilterCondition(typeof(T).Name, string.Format("{0} eq {1}", @event.GetPropertyName(property), new ODataExpressionValue(value)), @event.GetPropertyName(property));
        }

        public static FilterCondition BuildFilterForArrayProperty<T, TP, TV>(Expression<Func<T, TP[]>> property, Expression<Func<TP, TV>> arrayProperty, TV value)
            where T : new()
            where TP : new()
        {
            var @event = new T();
            var tp = new TP();

            //Children/any(Child: Child/Name eq 'Najaf Shaikh')
            var expression = string.Format("{0}/any({1}: {1}/{2} eq {3})", @event.GetPropertyName(property), typeof(TP).Name, tp.GetPropertyName(arrayProperty), new ODataExpressionValue(value));
            return new FilterCondition(typeof(T).Name, typeof(TP).Name, expression, @event.GetPropertyName(property));

        }

        /// <summary>
        /// Builds the odata expression for all filter items provided
        /// </summary>
        /// <param name="filterConditions">odata filter items</param>
        /// <returns>string odata filter expression</returns>
        public static string BuildExpression(IList<FilterCondition> filterConditions)
        {
            if (filterConditions == null || !filterConditions.Any()) return null;

            var builder = new StringBuilder();
            var expAdded = false;

            foreach (var filterGrp in filterConditions.Where(x => !string.IsNullOrEmpty(x.FilterGroup) && !string.IsNullOrEmpty(x.Expression))
                                             .GroupBy(x => x.FilterGroup)
                                             .OrderBy(x => x.Count()))
            {
                var propertyCount = filterGrp.Count();

                if (propertyCount == 0) continue;

                if (expAdded)
                {
                    builder.Append(" and ");
                    expAdded = false;
                }

                if (propertyCount > 1)
                    builder.Append("(");

                for (var i = 0; i < propertyCount; i++)
                {

                    var expression = filterGrp.ElementAt(i).Expression;

                    builder.Append(propertyCount > 1 && i != (propertyCount - 1) ? expression + " or " : expression);
                    expAdded = true;
                }

                if (propertyCount > 1)
                    builder.Append(")");
            }

            return builder.ToString();
        }
    }
}
