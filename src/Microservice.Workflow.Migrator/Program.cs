using System;
using Microservice.Workflow.Migrator.Impl;
using NDesk.Options;
using NLog;

namespace Microservice.Workflow.Migrator
{
    internal class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var configuration = new MigrateConfiguration();
            var showHelp = false;

            OptionSet options = null;
            try
            {
                options = new OptionSet
                {
                    {
                        "x=", "Perform migration of templates and instances", o =>
                        {
                            var type = (MigrationType) Enum.Parse(typeof (MigrationType), o);
                            configuration.Type = type;
                        }
                    },
                    {
                        "i=|instanceId=", "Id of workflow instance to migrate", o =>
                        {
                            var instanceId = Guid.Parse(o);
                            configuration.InstanceId = instanceId;
                        }
                    },
                    {
                        "t=|templateId=", "Id of workflow template to migrate", o =>
                        {
                            var templateId = int.Parse(o);
                            configuration.TemplateId = templateId;
                        }
                    },
                    {
                        "n=", "Id of tenant to filter for instances", o =>
                        {
                            var tenantId = int.Parse(o);
                            configuration.TenantId = tenantId;
                        }
                    },
                    {"h|?|help", "Show the help", o => showHelp = o != null}
                };
                options.Parse(args);
            }
            catch (Exception ex)
            {
                WriteMessage(ex.Message);
            }

            var noExecute = !(configuration.Type != MigrationType.None || configuration.MigrateInstances || configuration.MigrateTemplates);
            if (showHelp || noExecute)
            {
                ShowHelp(options);
                return;
            }

            if (configuration.MigrateTemplates)
                Render("Templates", () => Execute(new TemplateMigrator(configuration)));
            if (configuration.MigrateInstances)
                Render("Instances", () => Execute(new InstanceMigrator(configuration)));
            if(configuration.MigrateInstanceDelays)
                Render("InstanceDelays", () => Execute(new InstanceDelayMigrator(configuration)));

        }

        private static void Execute(IMigrator impl)
        {
            try
            {
                var migrator = new MigrationPresenter();
                var task = migrator.Execute(impl);
                task.GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                WriteMessage(ex.Message);
            }
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: Microservice.Workflow.Migrator [OPTIONS]+");
            Console.WriteLine("Migrate workflow templates and instances");
            Console.WriteLine();
            if (p == null) return;
            Console.WriteLine("Options:");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void WriteMessage(string message)
        {
            Console.WriteLine();
            WriteBreak();
            Console.WriteLine(message);
            Console.WriteLine();
            WriteBreak();
        }

        private static void WriteBreak()
        {
            Console.WriteLine(new string('-', Console.BufferWidth));
        }

        private static void Render(string name, Action action)
        {
            Console.WriteLine();
            Console.WriteLine(name);
            WriteBreak();
            action();
            Console.WriteLine();
        }
    }
} ;