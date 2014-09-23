﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Arbor.Aesculus.Core;
using Arbor.X.Core;
using Arbor.X.Core.BuildVariables;
using Arbor.X.Core.IO;
using Arbor.X.Core.Logging;
using Arbor.X.Core.Tools;
using Arbor.X.Core.Tools.Testing;
using Machine.Specifications;

namespace Arbor.X.Tests.Integration.Tests.MSpec
{
    [Subject(typeof (MSpecTestRunner))]
    [Tags("Arbor_X_Recursive")]
    public class when_running_mspec_on_self
    {
        static MSpecTestRunner testRunner;
        static List<IVariable> variables = new List<IVariable>();
        static ExitCode ExitCode;
        static string mspecReports;

        Establish context = () =>
        {
            string root = Path.Combine(VcsPathHelper.FindVcsRootPath(), "src");

            string combine = Path.Combine(root, "Arbor.X.Tests.Integration", "bin", "debug");

            string tempPath = Path.Combine(Path.GetTempPath(), "Arbor.X", "MSpec", Guid.NewGuid().ToString());

            DirectoryInfo tempDirectory = new DirectoryInfo(tempPath).EnsureExists();

            exitCode = DirectoryCopy.CopyAsync(combine, tempDirectory.FullName).Result;

            testRunner = new MSpecTestRunner();
            variables.Add(new EnvironmentVariable(WellKnownVariables.ExternalTools,
                Path.Combine(VcsPathHelper.FindVcsRootPath(), "tools", "external")));

            variables.Add(new EnvironmentVariable(WellKnownVariables.SourceRootOverride, tempDirectory.FullName));
            variables.Add(new EnvironmentVariable(WellKnownVariables.SourceRoot, tempDirectory.FullName));


            mspecReports = Path.Combine(tempDirectory.FullName, "MSpecReports");

            new DirectoryInfo(mspecReports).EnsureExists();

            variables.Add(new EnvironmentVariable(WellKnownVariables.ExternalTools_MSpec_ReportPath, mspecReports));
        };

        Because of =
            () =>
                ExitCode =
                    testRunner.ExecuteAsync(new ConsoleLogger {LogLevel = LogLevel.Verbose}, variables,
                        new CancellationToken()).Result;

        It shoud_have_created_html_report = () =>
        {
            DirectoryInfo reports = new DirectoryInfo(mspecReports);
            DirectoryInfo htmlDirectory = reports.GetDirectories()
                .SingleOrDefault(dir => dir.Name.Equals("html", StringComparison.InvariantCultureIgnoreCase));

            var files = reports.GetFiles("*.html", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                Console.WriteLine(fileInfo.FullName);
            }

            htmlDirectory.ShouldNotBeNull();
        };
        It shoud_have_created_xml_report = () =>
        {
            DirectoryInfo reports = new DirectoryInfo(mspecReports);

            var files = reports.GetFiles("*.xml", SearchOption.AllDirectories);

            foreach (var fileInfo in files)
            {
                Console.WriteLine(fileInfo.FullName);
            }

            files.Length.ShouldNotEqual(0);
        };

        It should_Behaviour = () => ExitCode.IsSuccess.ShouldBeTrue();
        static ExitCode exitCode;
    }
}