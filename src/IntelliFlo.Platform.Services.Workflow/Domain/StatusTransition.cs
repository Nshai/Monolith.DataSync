using System.Globalization;

namespace IntelliFlo.Platform.Services.Workflow.Domain
{
    public class StatusTransition
    {
        private const string TransitionFormat = "{0} to {1}";

        public StatusTransition(int fromStatusId, int toStatusId)
        {
            FromStatusId = fromStatusId;
            ToStatusId = toStatusId;
        }

        public int? FromStatusId { get; set; }
        public int? ToStatusId { get; set; }

        public override string ToString()
        {
            return string.Format(TransitionFormat, FormatStatusValue(FromStatusId), FormatStatusValue(ToStatusId));
        }

        private static string FormatStatusValue(int? value)
        {
            return !value.HasValue ? "null" : value.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}