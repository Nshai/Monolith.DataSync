using System;
using System.Collections.Generic;
using System.Linq;
using Microservice.Workflow.Collaborators.v1.Events;

namespace Microservice.Workflow.Domain
{
    public class LeadCreatedTrigger : BaseTrigger
    {
        private const string GeneralInsurance = "General Insurance";
        private const string LifeAssurance = "Life Assurance";
        private const string Investments = "Investments";
        private const string Protection = "Protection";
        private const string Pensions = "Pensions";
        private const string Mortgage = "Mortgage";

        public LeadCreatedTrigger() : base("LeadCreated", WorkflowRelatedTo.Lead){}

        public override void PopulateFromRequest(CreateTemplateTrigger request)
        {
            GeneralInsuranceResponses = request.GeneralInsuranceResponses;
            LifeAssuranceResponses = request.LifeAssuranceResponses;
            ProtectionResponses = request.ProtectionResponses;
            InvestmentResponses = request.InvestmentResponses;
            PensionResponses = request.PensionResponses;
            MortagageResponses = request.MortagageResponses;
        }

        public override void PopulateDocument(TemplateTrigger document)
        {
            document.GeneralInsuranceResponses = GeneralInsuranceResponses;
            document.LifeAssuranceResponses = LifeAssuranceResponses;
            document.ProtectionResponses = ProtectionResponses;
            document.InvestmentResponses = InvestmentResponses;
            document.PensionResponses = PensionResponses;
            document.MortagageResponses = MortagageResponses;
        }

        public override IEnumerable<BaseTriggerProperty> Serialize()
        {
            foreach (var id in GeneralInsuranceResponses ?? new int[0])
            {
                yield return new LeadInterestTriggerProperty
                {
                    InterestId = id,
                    InterestName = GeneralInsurance
                };
            }

            foreach (var id in LifeAssuranceResponses ?? new int[0])
            {
                yield return new LeadInterestTriggerProperty
                {
                    InterestId = id,
                    InterestName = LifeAssurance
                };
            }

            foreach (var id in ProtectionResponses ?? new int[0])
            {
                yield return new LeadInterestTriggerProperty
                {
                    InterestId = id,
                    InterestName = Protection
                };
            }

            foreach (var id in InvestmentResponses ?? new int[0])
            {
                yield return new LeadInterestTriggerProperty
                {
                    InterestId = id,
                    InterestName = Investments
                };
            }

            foreach (var id in PensionResponses ?? new int[0])
            {
                yield return new LeadInterestTriggerProperty
                {
                    InterestId = id,
                    InterestName = Pensions
                };
            }

            foreach (var id in MortagageResponses ?? new int[0])
            {
                yield return new LeadInterestTriggerProperty
                {
                    InterestId = id,
                    InterestName = Mortgage
                };
            }
        }

        public override void Deserialize(IList<BaseTriggerProperty> triggerProperties)
        {
            GeneralInsuranceResponses = GetInterestResponses(triggerProperties, GeneralInsurance);
            LifeAssuranceResponses = GetInterestResponses(triggerProperties, LifeAssurance);
            ProtectionResponses = GetInterestResponses(triggerProperties, Protection);
            InvestmentResponses = GetInterestResponses(triggerProperties, Investments);
            PensionResponses = GetInterestResponses(triggerProperties, Pensions);
            MortagageResponses = GetInterestResponses(triggerProperties, Mortgage);

        }

        public int[] GeneralInsuranceResponses { get; set; }
        public int[] LifeAssuranceResponses { get; set; }
        public int[] ProtectionResponses { get; set; }
        public int[] InvestmentResponses { get; set; }
        public int[] PensionResponses { get; set; }
        public int[] MortagageResponses { get; set; }

        private static int[] GetInterestResponses(IEnumerable<BaseTriggerProperty> triggerProperties, string interestName)
        {
            var properties = triggerProperties.OfType<LeadInterestTriggerProperty>().Where(p => String.Equals(p.InterestName, interestName, StringComparison.InvariantCultureIgnoreCase));
            return properties.Select(p => p.InterestId).ToArray();
        }

        public override IEnumerable<FilterCondition> GetFilter()
        {
            foreach (var id in GeneralInsuranceResponses ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForArrayProperty<LeadCreated, InterestArea, int>(x => x.InterestAreas, x => x.InterestAreaId, id);
            }

            foreach (var id in LifeAssuranceResponses ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForArrayProperty<LeadCreated, InterestArea, int>(x => x.InterestAreas, x => x.InterestAreaId, id);
            }

            foreach (var id in ProtectionResponses ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForArrayProperty<LeadCreated, InterestArea, int>(x => x.InterestAreas, x => x.InterestAreaId, id);
            }

            foreach (var id in InvestmentResponses ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForArrayProperty<LeadCreated, InterestArea, int>(x => x.InterestAreas, x => x.InterestAreaId, id);
            }

            foreach (var id in PensionResponses ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForArrayProperty<LeadCreated, InterestArea, int>(x => x.InterestAreas, x => x.InterestAreaId, id);
            }

            foreach (var id in MortagageResponses ?? new int[0])
            {
                yield return ODataBuilder.BuildFilterForArrayProperty<LeadCreated, InterestArea, int>(x => x.InterestAreas, x => x.InterestAreaId, id);
            }
        }
    }
}