using System;
using System.Activities;

namespace Microservice.Workflow.v1.Activities
{
    public sealed class ResolveDelay : NativeActivity
    {
        public InArgument<TimeSpan> Period { get; set; }
        public InArgument<bool> BusinessDaysOnly { get; set; }
        public OutArgument<DateTime> ExpiryDate { get; set; }

        protected override void Execute(NativeActivityContext context)
        {
            var period = Period.Get(context);
            var businessDaysOnly = BusinessDaysOnly.Get(context);
            var startDateUtc = DateTime.UtcNow;
            var expiryDateUtc = DateCalculator.AddDays(startDateUtc, period, businessDaysOnly);

            ExpiryDate.Set(context, expiryDateUtc);
        }
    }
}
