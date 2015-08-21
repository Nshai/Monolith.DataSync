using System;
using System.Activities;
using System.Activities.Expressions;
using System.Activities.XamlIntegration;
using System.IO;
using System.Linq;
using System.ServiceModel.Activities;
using System.Xaml;
using IntelliFlo.Platform.NHibernate;
using log4net;

namespace Microservice.Workflow.Domain
{
    public class TemplateDefinition : EqualityAndHashCodeProvider<TemplateDefinition, Guid>
    {
        public const int DefaultVersion = 1;
        private readonly ILog logger = LogManager.GetLogger(typeof(TemplateDefinition));

        public virtual int TenantId { get; set; }
        public virtual string Name { get; set; }
        public virtual string Definition { get; set; }
        public virtual DateTime DateUtc { get; set; }
        public virtual bool InUse { get; set; }
        public virtual int Version { get; set; }
               
        /// <summary>
        /// This is necessary if we have C# expressions embedded in the workflow definition
        /// However in our templates, the assumption is that all the expressions are within Activities that are defined within the 
        /// IntelliFlo.Workflow assembly and therefore will already be compiled
        /// Consequently this compilation step is unnecessary
        /// </summary>
        /// <returns></returns>
        public virtual bool Compile()
        {
            using (var reader = new StringReader(Definition))
            using (var xamlReader = ActivityXamlServices.CreateBuilderReader(new XamlXmlReader(reader)))
            {
                try
                {
                    var workflow = XamlServices.Load(xamlReader) as WorkflowService;
                    if (workflow == null)
                        return false;
                    
                    return CompileExpressions(workflow.Body);
                }
                catch (XamlException ex)
                {
                    logger.Error(string.Format("Template {0} could not be loaded", Id), ex);
                    return false;
                }
                catch (InvalidOperationException ex)
                {
                    logger.Error("Template compilation failed", ex);
                    return false;
                }
            }
        }

        private bool CompileExpressions(Activity activity)
        {
            var compiledExpressionRoot = CompiledExpressionInvoker.GetCompiledExpressionRoot(activity) as ICompiledExpressionRoot;
            if (compiledExpressionRoot != null)
                return true;
            
            // activityName is the Namespace.Type of the activity that contains the
            // C# expressions.
            var activityName = activity.GetType().ToString();

            // Split activityName into Namespace and Type.Append _CompiledExpressionRoot to the type name
            // to represent the new type that represents the compiled expressions.
            // Take everything after the last . for the type name.
            var activityType = Enumerable.Last<string>(activityName.Split('.')) + "_CompiledExpressionRoot";
            // Take everything before the last . for the namespace.
            var activityNamespace = string.Join(".", Enumerable.Reverse<string>(activityName.Split('.')).Skip(1).Reverse());

            // Create a TextExpressionCompilerSettings.
            var settings = new TextExpressionCompilerSettings
            {
                Activity = activity,
                Language = "C#",
                ActivityName = activityType,
                ActivityNamespace = activityNamespace,
                RootNamespace = null,
                GenerateAsPartialClass = false,
                AlwaysGenerateSource = true,
                ForImplementation = false
            };

            // Compile the C# expression.
            var results = new TextExpressionCompiler(settings).Compile();

            // Any compilation errors are contained in the CompilerMessages.
            if (results.HasErrors)
            {
                foreach (var message in results.CompilerMessages)
                {
                    logger.ErrorFormat("Compilation failed : {0} - {1}", message.Number, message.Message);
                }
                return false;
            }

            // Create an instance of the new compiled expression type.
            compiledExpressionRoot = Activator.CreateInstance(results.ResultType, new object[] { activity }) as ICompiledExpressionRoot;

            // Attach it to the activity.
            CompiledExpressionInvoker.SetCompiledExpressionRoot(activity, compiledExpressionRoot);

            return true;
        }
    }
}