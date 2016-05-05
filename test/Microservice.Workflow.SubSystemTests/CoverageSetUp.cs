using NUnit.Framework;
using Reassure.Logging;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Microservice.Workflow.SubSystemTests
{
    public class CoverageSetUp
    {
        [SetUp]
        public void SetCoverageUp()
        {
            var logger = TestLogger.GlobalLogger();
            logger.Loggers = new ITestLogger[] {
                new DebugLogger()//optional
                {
                    Formatter = new YamlLogFormatter() { ExcludeRawContent = true }
                },
                new FileLogger()
                {
                    Formatter = new YamlLogFormatter() { ExcludeBody = true }//recommended setup
                }
            };
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
#if DEBUG
            var yamlFileName = "reassure.yaml";
            var swaggerEndPoint = ConfigurationManager.AppSettings["Reassure:ServiceBaseAddress"] + "/docs/v1/.metadata";
            var outputPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "reassure.coverage.exe",
                    Arguments = string.Format("{0} {1} -o:{2}", yamlFileName, swaggerEndPoint, outputPath),
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();
            process.Close();

            if (File.Exists("coverage_full.html"))
            {
                Process.Start("coverage_full.html");
            }
#endif
        }
    }
}
