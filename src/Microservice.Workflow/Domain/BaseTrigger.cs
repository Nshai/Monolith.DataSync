using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microservice.Workflow.Domain
{
    public abstract class BaseTrigger
    {
        private readonly string eventName;
        private readonly WorkflowRelatedTo? relatedTo;

        protected BaseTrigger(string eventName = null, WorkflowRelatedTo? relatedTo = null)
        {
            this.eventName = eventName;
            this.relatedTo = relatedTo;
        }

        public WorkflowRelatedTo? RelatedTo
        {
            get { return relatedTo; }
        }

        public string EventName
        {
            get { return eventName; }
        }

        public abstract void PopulateFromRequest(CreateTemplateTrigger request);
        public abstract void PopulateDocument(TemplateTrigger document);
        public abstract IEnumerable<BaseTriggerProperty> Serialize();
        public abstract void Deserialize(IList<BaseTriggerProperty> triggerProperties);
        public abstract IEnumerable<FilterCondition> GetFilter();

        protected TResult? GetPropertyValue<T, TResult>(IEnumerable<BaseTriggerProperty> triggerProperties, Func<T, TResult?> getProperty)
            where T : BaseTriggerProperty
            where TResult : struct
        {
            var property = triggerProperties.OfType<T>().SingleOrDefault();
            return property != null ? getProperty(property) : null;
        }

        protected TResult[] GetPropertyArray<T, TResult>(IEnumerable<BaseTriggerProperty> triggerProperties, Func<T, TResult> getProperty)
            where T : BaseTriggerProperty
            where TResult : struct
        {
            var properties = triggerProperties.OfType<T>();
            return properties.Select(getProperty).ToArray();
        }

        protected T SetPropertyValue<T, TResult>(Expression<Func<T, object>> setProperty, TResult? value)
            where T : BaseTriggerProperty, new()
            where TResult : struct
        {
            if (!value.HasValue) return null;

            var property = new T();
            property.SetPropertyValue(setProperty, value);
            return property;
        }

        protected IEnumerable<T> SetPropertyArray<T, TResult>(Expression<Func<T, object>> setProperty, IList<TResult> value)
            where T : BaseTriggerProperty, new()
            where TResult : struct
        {
            return value == null || !value.Any() ? new T[0] : value.Select(id => SetPropertyValue<T, TResult>(setProperty, id));
        }
    }
}