using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using NUnit.Framework;
using Reassure.Logging;

namespace Microservice.Workflow.SubSystemTests
{
    // https://confluence.intelliflo.com/display/DEV/Code+Coverage
    // If coverage is not working, see how to capture reassure.coverage.exe output here:
    // https://confluence.intelliflo.com/display/DEV/Code+Coverage#CodeCoverage-Debuggingreassure.coverage
    public class CoverageSetUp
    {
        private static string AssemblyDir { get; set; }
        private static string ReassureYamlFile { get; set; }
        const string yamlFileName = "reassure.yaml";

        static CoverageSetUp()
        {
            AssemblyDir = GetAssemblyDirectory();
            ReassureYamlFile = GetAssemblyDirectory() + "/" + yamlFileName;
        }

        [SetUp]
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

        [TearDown]
        public void RunAfterAnyTests()
        {
#if DEBUG
            var swaggerEndPointV1 = ConfigurationManager.AppSettings["Reassure:ServiceBaseAddress"] + "/docs/v1/swagger";

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "reassure.coverage.exe",
                    Arguments = $"{yamlFileName} -s:{swaggerEndPointV1} -o:{AssemblyDir}",
                    RedirectStandardInput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();
            process.Close();

            // reports will be in subsys/bin/ folder
            // e.g. coverage_full_v1.html  or coverage_full_v2.html
#endif
        }


        private static string GetAssemblyDirectory()
        {
            var codeBase = typeof(CoverageSetUp).Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}