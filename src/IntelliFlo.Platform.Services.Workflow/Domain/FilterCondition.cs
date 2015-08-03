namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class FilterCondition
    {
        public FilterCondition(string eventName, string expression, string filterGroup)
            : this(eventName, null, expression, filterGroup)
        { }

        public FilterCondition(string eventName, string arrayProperty, string expression, string filterGroup)
        {
            EventName = eventName;
            ArrayProperty = arrayProperty;
            Expression = expression;
            FilterGroup = filterGroup;
        }

        public string ArrayProperty { get; set; }
        public string EventName { get; private set; }
        public string Expression { get; private set; }
        public string FilterGroup { get; set; }
    }
}