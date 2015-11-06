using System.Linq;
using AutoMapper;
using IntelliFlo.Platform.AutoMapper;
using Microservice.Workflow.Domain;
using Microservice.Workflow.v1.Contracts;

namespace Microservice.Workflow.Modules
{
    public class WorkflowAutoMapperModule : IModule
    {
        public void Load()
        {
            Mapper.CreateMap<Instance, InstanceDocument>()
                .ForMember(dest => dest.Template, opt => opt.MapFrom(src => new InstanceDocument.InstanceTemplate(){ TemplateId = src.Template.Id, Name = src.Template.Name }));

            Mapper.CreateMap<InstanceHistory, InstanceHistoryDocument>()
                .ForMember(dest => dest.InstanceHistoryId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.TimestampUtc));

            Mapper.CreateMap<InstanceStep, InstanceStepDocument>()
                .ForMember(dest => dest.InstanceStepId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.TimestampUtc));

            Mapper.CreateMap<Template, TemplateDocument>()
                .ForMember(dest => dest.TemplateVersionId, opt => opt.MapFrom(src => src.CurrentVersion.Id));

            Mapper.CreateMap<Template, TemplateMigrationDocument>()
                .ForMember(dest => dest.EventSubscriptionId, opt => opt.MapFrom(src => src.TriggerSet.EventSubscriptionId));

            Mapper.CreateMap<TemplateCategory, TemplateDocument.TemplateCategory>()
                .ForMember(dest => dest.TemplateCategoryId, opt => opt.MapFrom(src => src.Id));

           Mapper.CreateMap<TemplateCategory, TemplateCategoryDocument>()
                .ForMember(dest => dest.TemplateCategoryId, opt => opt.MapFrom(src => src.Id));

            Mapper.CreateMap<TemplateDefinition, TemplateDefinitionDocument>()
                .ForMember(dest => dest.TemplateId, opt => opt.MapFrom(src => src.Id));

            Mapper.CreateMap<IWorkflowStep, TemplateStepDocument>()
                .Include<CreateTaskStep, TemplateStepDocument>()
                .Include<DelayStep, TemplateStepDocument>();

            Mapper.CreateMap<CreateTaskStep, TemplateStepDocument>()
                .ForMember(dest => dest.Type, opt => opt.UseValue(StepType.CreateTask))
                .ForMember(dest => dest.AssignedTo, opt => opt.MapFrom(src => src.AssignedTo))
                .ForMember(dest => dest.Delay, opt => opt.MapFrom(src => src.DueDelay))
                .ForMember(dest => dest.DelayBusinessDays, opt => opt.MapFrom(src => src.DueDelayBusinessDays));

            Mapper.CreateMap<DelayStep, TemplateStepDocument>()
                .ForMember(dest => dest.Type, opt => opt.UseValue(StepType.Delay))
                .ForMember(dest => dest.Delay, opt => opt.MapFrom(src => src.Days))
                .ForMember(dest => dest.DelayBusinessDays, opt => opt.MapFrom(src => src.BusinessDays));

            Mapper.CreateMap<TemplateRole, TemplateRoleDocument>()
                .ForMember(dest => dest.TemplateId, opt => opt.MapFrom(src => src.TemplateVersion.Template.Id))
                .ForMember(dest => dest.TemplateVersionId, opt => opt.MapFrom(src => src.TemplateVersion.Id));

            Mapper.CreateMap<Template, TemplateExtDocument>()
                .ForMember(dest => dest.TemplateVersionId, opt => opt.MapFrom(src => src.CurrentVersion.Id))
                .ForMember(dest => dest.RoleIds, opt => opt.MapFrom(src => src.Roles.Select(r => r.RoleId)));

            Mapper.CreateMap<IWorkflowStep, TemplateExtDocument.TemplateStep>()
                .Include<CreateTaskStep, TemplateExtDocument.TemplateStep>()
                .Include<DelayStep, TemplateExtDocument.TemplateStep>();

            Mapper.CreateMap<CreateTaskStep, TemplateExtDocument.TemplateStep>()
                .ForMember(dest => dest.Type, opt => opt.UseValue(StepType.CreateTask))
                .ForMember(dest => dest.Delay, opt => opt.MapFrom(src => src.DueDelay))
                .ForMember(dest => dest.DelayBusinessDays, opt => opt.MapFrom(src => src.DueDelayBusinessDays));

            Mapper.CreateMap<DelayStep, TemplateExtDocument.TemplateStep>()
                .ForMember(dest => dest.Type, opt => opt.UseValue(StepType.Delay))
                .ForMember(dest => dest.Delay, opt => opt.MapFrom(src => src.Days))
                .ForMember(dest => dest.DelayBusinessDays, opt => opt.MapFrom(src => src.BusinessDays));

            Mapper.CreateMap<TemplateCategory, TemplateExtDocument.TemplateExtCategory>()
                .ForMember(dest => dest.TemplateCategoryId, opt => opt.MapFrom(src => src.Id));

            Mapper.CreateMap<TemplateTrigger.StatusTransitionDefinition, TemplateTriggerDocument.StatusTransitionDefinition>();

            Mapper.CreateMap<CreateTemplateTriggerRequest, CreateTemplateTrigger>();
            Mapper.CreateMap<CreateTemplateTriggerRequest.CreateTemplateTriggerStatusTransitionDefinition, CreateTemplateTrigger.CreateTemplateTriggerStatusTransitionDefinition>();

            Mapper.CreateMap<TemplateTrigger, TemplateTriggerDocument>();

            Mapper.CreateMap<TemplateStepPatchRequest, TemplateStepPatch>();

            Mapper.CreateMap<CreateTemplateStepRequest, CreateTemplateStep>();

            Mapper.CreateMap<TemplateRegistration, TemplateRegistrationDocument>();
        }
    }
}