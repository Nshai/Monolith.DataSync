using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Reassure.Coverage;
using Reassure.Logging;
using Reassure.Stubs;

namespace Microservice.Workflow.SubSystemTests
{
    public class CoverageSetUp
    {
        private static string AssemblyDir { get; set; } = GetAssemblyDirectory();
        private static string ReassureYamlFile { get; set; } = AssemblyDir + "/" + YamlFileName;
        private const string YamlFileName = "reassure.yaml";
        public static string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{environmentName}.json", true, true)
            .AddEnvironmentVariables()
            .Build();

        [OneTimeSetUp]
        public void SetCoverageUp()
        {
            var logger = TestLogger.GlobalLogger();
            logger.Loggers = new ITestLogger[] {
                new DebugLogger()
                {
                    Formatter = new YamlLogFormatter() { ExcludeRawContent = true }
                },
                new FileLogger(ReassureYamlFile)
                {
                    Formatter = new YamlLogFormatter() { ExcludeBody = true }
                }
            };
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
#if DEBUG
            var swaggerEndPoint = Configuration["Reassure:ServiceBaseAddress"] + "/docs/v1/swagger";

            CoverageRunner.Run(
                YamlFileName,
                AssemblyDir,
                new[] { swaggerEndPoint },
                new string[0],
                new string[0]);

            const string report = "coverage_full.html";

            if (File.Exists(report))
            {
                Process.Start(new ProcessStartInfo { UseShellExecute = true, FileName = report });
            }
#endif
            Stub.DefaultProviderFactory().Reset().GetAwaiter().GetResult();
        }

        private static string GetAssemblyDirectory()
        {
            var codeBase = typeof(Config).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
