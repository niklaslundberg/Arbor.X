﻿using System; using Serilog;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Arbor.Processing.Core;
using Arbor.X.Core.IO;

using Machine.Specifications;
using Serilog.Core;

namespace Arbor.X.Tests.Integration.ProcessRunner
{
    [Subject(typeof(Processing.ProcessRunner))]
    [Tags(Core.Tools.Testing.MSpecInternalConstants.RecursiveArborXTest)]
    public class when_running_a_failing_process
    {
        static string testPath;
        static ILogger logger = Logger.None;
        static ExitCode exitCode;

        Cleanup after = () =>
        {
            if (File.Exists(testPath))
            {
                File.Delete(testPath);
            }
        };

        Establish context = () =>
        {
            testPath = Path.Combine(Path.GetTempPath(), $"{DefaultPaths.TempPathPrefix}Test_fail.tmp.bat");
            const string batchContent = @"@ECHO OFF
EXIT /b 3
";
            File.WriteAllText(testPath, batchContent, Encoding.Default);
        };

        Because of = () => RunAsync().Wait();

        It should_return_exit_code_from_process = () => exitCode.Result.ShouldEqual(3);

        static async Task RunAsync()
        {
            try
            {
                exitCode =
                    await
                        Processing.ProcessRunner.ExecuteAsync(testPath,
                            standardOutLog: (message, prefix) => logger.Information(message, "STANDARD"),
                            standardErrorAction: (message, prefix) => logger.Error(message, "ERROR"),
                            toolAction: (message, prefix) => logger.Information(message, "TOOL"),
                            verboseAction: (message, prefix) => logger.Information(message, "VERBOSE")).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }
    }
}
