using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microservice.Workflow.Domain;
using NUnit.Framework;

namespace Microservice.Workflow.Tests
{
    [TestFixture]
    public class TemplateTriggerTests
    {
        private const int TenantId = 1;
        
        [Test]
        public void WhenAddClientTriggersThenStoredCorrectly()
        {
            var triggerSet = GetTriggerSet(new CreateTemplateTrigger
            {
                Type = TriggerType.OnClientCreation.ToString(),
                ClientStatusId = 1,
                ClientCategories = new[] {2, 3, 4}
            });

            var serializedProperties = Serialize(triggerSet.PropertyList);
            var deserializedProperties = Deserialize<TriggerPropertyList>(serializedProperties);

            var deserializedTriggerSet = new TemplateTriggerSet(new TemplateVersion(), TenantId, TriggerType.OnClientCreation)
            {
                PropertyList = deserializedProperties
            };

            var t1 = triggerSet.Trigger as ClientCreatedTrigger;
            var t2 = deserializedTriggerSet.Trigger as ClientCreatedTrigger;

            Assert.AreEqual(t1.ClientStatusId, t2.ClientStatusId);
            Assert.AreEqual(t1.ClientCategories, t2.ClientCategories);
        }

        [Test]
        public void WhenSerializeClientCreationTriggerToODataThenFormattedCorrectly()
        {
            var expression = BuildODataExpression(new CreateTemplateTrigger
            {
                Type = TriggerType.OnClientCreation.ToString(),
                ClientStatusId = 1,
                ClientCategories = new[] {2, 3, 4}
            });

            Assert.AreEqual("ServiceStatusId eq 1 and (CategoryId eq 2 or CategoryId eq 3 or CategoryId eq 4)", expression);
        }

        /// <summary>
        /// This highlights a issues with a poorly named property - IsPreExisting
        /// In the UI, the selection is New Business Only.  We invert this selection and set as IsPreExisting.
        /// However when we convert to the filter we apply it directly so we end up  filtering for either pre-existing or not pre-existing. 
        /// But what we want  is to filter for not pre-existing or apply no filter.
        /// </summary>
        /// <param name="newBusinessOnly"></param>
        /// <returns></returns>
        [TestCase(true, ExpectedResult = "IsPreExisting eq false and (ProviderId eq 1 or ProviderId eq 2 or ProviderId eq 3) and (ProductTypeId eq 4 or ProductTypeId eq 5 or ProductTypeId eq 6)")]
        [TestCase(false, ExpectedResult = "(ProviderId eq 1 or ProviderId eq 2 or ProviderId eq 3) and (ProductTypeId eq 4 or ProductTypeId eq 5 or ProductTypeId eq 6)")]
        public string WhenSerializePlanCreationTriggerToODataThenFormattedCorrectly(bool newBusinessOnly)
        {
            var expression = BuildODataExpression(new CreateTemplateTrigger()
            {
                Type = TriggerType.OnPlanCreation.ToString(),
                PlanProviders = new[] {1,2,3},
                PlanTypes = new [] {4,5,6},
                IsPreExisting = !newBusinessOnly
            });

            return expression;
        }


        [Test]
        public void WhenSerializeClientCreationTriggerWithNoFiltersToODataThenFormattedCorrectly()
        {
            var expression = BuildODataExpression(new CreateTemplateTrigger
            {
                Type = TriggerType.OnClientCreation.ToString()
            });

            Assert.IsNull(expression);
        }


        [Test]
        public void WhenSerializeClientDeletionTriggerToODataThenFormattedCorrectly()
        {
            var expression = BuildODataExpression(new CreateTemplateTrigger
            {
                Type = TriggerType.OnClientDeletion.ToString()
            });

            Assert.IsNullOrEmpty(expression);
        }

        [Test]
        public void WhenSerializeLeadCreationTriggerToODataThenFormattedCorrectly()
        {
            var expression = BuildODataExpression(new CreateTemplateTrigger
            {
                Type = TriggerType.OnLeadCreation.ToString(),
                GeneralInsuranceResponses = new[] { 1,2,3},
                LifeAssuranceResponses = new[] {4,5}
            });

            Assert.AreEqual("(InterestAreas/any(InterestArea: InterestArea/InterestAreaId eq 1) or InterestAreas/any(InterestArea: InterestArea/InterestAreaId eq 2) or InterestAreas/any(InterestArea: InterestArea/InterestAreaId eq 3) or InterestAreas/any(InterestArea: InterestArea/InterestAreaId eq 4) or InterestAreas/any(InterestArea: InterestArea/InterestAreaId eq 5))", expression);
        }
        
        [TestCase(TriggerType.OnPlanAddedToScheme, "<TriggerList xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Triggers><GroupSchemePlanAddedTrigger><TriggerForNewMembersOnly>true</TriggerForNewMembersOnly><TriggerForRejoin>true</TriggerForRejoin></GroupSchemePlanAddedTrigger></Triggers></TriggerList>")]
        [TestCase(TriggerType.OnClientStatusUpdate, "<TriggerList xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Triggers><ClientStatusTransitionTrigger><StatusFromId>12665</StatusFromId><StatusToId>13073</StatusToId></ClientStatusTransitionTrigger></Triggers></TriggerList>")]
        [TestCase(TriggerType.OnClientCreation, "<TriggerList xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Triggers><ClientStatusTrigger><StatusId>1680</StatusId></ClientStatusTrigger><ClientCategoryTrigger><CategoryId>1</CategoryId></ClientCategoryTrigger></Triggers></TriggerList>")]
        [TestCase(TriggerType.OnPlanCreation, "<TriggerList xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Triggers><PlanTypeTrigger><ProductTypeId>15</ProductTypeId></PlanTypeTrigger><PlanProviderTrigger><ProviderId>4</ProviderId></PlanProviderTrigger><PlanNewBusinessTrigger><IsPreExisting>false</IsPreExisting></PlanNewBusinessTrigger></Triggers></TriggerList>")]
        [TestCase(TriggerType.OnPlanStatusUpdate, "<TriggerList xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><Triggers><PlanStatusTransitionTrigger><StatusFromId>12665</StatusFromId><StatusToId>13073</StatusToId></PlanStatusTransitionTrigger></Triggers></TriggerList>")]
        public void WhenDeserializeTriggerThenFormattedCorrectly(TriggerType type, string serialization)
        {
            var deserializedProperties = Deserialize<TriggerPropertyList>(serialization);
            var templateVersion = new TemplateVersion();
            var triggerSet = new TemplateTriggerSet(templateVersion, TenantId, type) {PropertyList = deserializedProperties};

            Assert.IsNotNull(triggerSet.Trigger);
            var trigger = triggerSet.Trigger;

            var clonedTriggerSet = new TemplateTriggerSet(templateVersion, TenantId, type)
            {
                Trigger = trigger
            };
            
            var reserialized = Serialize(clonedTriggerSet.PropertyList);
            Assert.AreEqual(serialization, reserialized);
        }

        [TestCase(true, false, ExpectedResult = "IsNewMember eq true and (ProviderId eq 3 or ProviderId eq 4) and (ProductTypeId eq 1 or ProductTypeId eq 2)")]
        [TestCase(false, true, ExpectedResult = "IsNewMember eq false and (ProviderId eq 3 or ProviderId eq 4) and (ProductTypeId eq 1 or ProductTypeId eq 2)")]
        [TestCase(false, false, ExpectedResult = "(ProviderId eq 3 or ProviderId eq 4) and (ProductTypeId eq 1 or ProductTypeId eq 2)")]
        [TestCase(true, true, ExpectedResult = "(ProviderId eq 3 or ProviderId eq 4) and (ProductTypeId eq 1 or ProductTypeId eq 2)")]
        public string WhenSerializeGroupSchemePlanAddedTriggerToODataThenFormattedCorrectly(bool newMembers, bool rejoin)
        {
            var expression = BuildODataExpression(new CreateTemplateTrigger
            {
                Type = TriggerType.OnPlanAddedToScheme.ToString(),
                PlanTypes = new[] { 1, 2},
                PlanProviders = new[] { 3, 4 },
                GroupSchemeNewMembers = newMembers,
                GroupSchemeMemberRejoin = rejoin
            });

            return expression;
        }

        private static TemplateTriggerSet GetTriggerSet(CreateTemplateTrigger request)
        {
            var triggerType = (TriggerType)Enum.Parse(typeof(TriggerType), request.Type);
            var triggerSet = new TemplateTriggerSet(new TemplateVersion(), TenantId, triggerType);

            var trigger = TemplateTriggerFactory.CreateFromRequest(request);
            triggerSet.Trigger = trigger;

            return triggerSet;
        }

        private string BuildODataExpression(CreateTemplateTrigger request)
        {
            var triggerSet = GetTriggerSet(request);
            IEnumerable<FilterCondition> filters = triggerSet.Trigger.GetFilter();
            return ODataBuilder.BuildExpression(filters.ToList());
        }

        private string Serialize<T>(T input)
        {
            var settings = new XmlWriterSettings
            {
                Indent = false, 
                OmitXmlDeclaration = true, 
                NewLineChars = "", 
                NewLineHandling = NewLineHandling.None
            };

            using (var writer = new StringWriter())
            using(var xmlWriter = XmlWriter.Create(writer, settings))
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(xmlWriter, input);
                return writer.ToString();
            }
        }

        private T Deserialize<T>(string serialization) where T : class
        {
            return new XmlSerializer(typeof(T)).Deserialize(new StringReader(serialization)) as T;
        }
    }
}