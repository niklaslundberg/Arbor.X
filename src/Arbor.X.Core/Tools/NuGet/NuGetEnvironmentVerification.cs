﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arbor.Build.Core.BuildVariables;
using Arbor.Build.Core.ProcessUtils;
using Arbor.Build.Core.Tools.EnvironmentVariables;
using Arbor.Processing;
using JetBrains.Annotations;
using Serilog;

namespace Arbor.Build.Core.Tools.NuGet
{
    [Priority(52)]
    [UsedImplicitly]
    public class NuGetEnvironmentVerification : EnvironmentVerification
    {
        public NuGetEnvironmentVerification() => RequiredValues.Add(WellKnownVariables.ExternalTools_NuGet_ExePath);

        protected override async Task<bool> PostVariableVerificationAsync(
            StringBuilder variableBuilder,
            IReadOnlyCollection<IVariable> buildVariables,
            ILogger logger)
        {
            IVariable variable =
                buildVariables.SingleOrDefault(item => item.Key == WellKnownVariables.ExternalTools_NuGet_ExePath);

            if (variable == null)
            {
                return false;
            }

            string nuGetExePath = variable.Value;

            bool fileExists = File.Exists(nuGetExePath);

            if (!fileExists)
            {
                variableBuilder.Append("NuGet.exe path '").Append(nuGetExePath).AppendLine("' does not exist");
            }
            else
            {
                bool nuGetUpdateEnabled =
                    buildVariables.GetBooleanByKey(WellKnownVariables.NuGetSelfUpdateEnabled, true);

                if (nuGetUpdateEnabled)
                {
                    logger.Verbose("NuGet self update is enabled by variable '{NuGetSelfUpdateEnabled}'",
                        WellKnownVariables.NuGetSelfUpdateEnabled);

                    await EnsureMinNuGetVersionAsync(nuGetExePath, logger).ConfigureAwait(false);
                }
                else
                {
                    logger.Verbose("NuGet self update is disabled by variable '{NuGetSelfUpdateEnabled}'",
                        WellKnownVariables.NuGetSelfUpdateEnabled);
                }
            }

            return fileExists;
        }

        private async Task EnsureMinNuGetVersionAsync(string nuGetExePath, ILogger logger)
        {
            var standardOut = new List<string>();
            ILogger versionLogger = InMemoryLoggerHelper.CreateInMemoryLogger((message, level) => standardOut.Add(message));

            try
            {
                IEnumerable<string> args = new List<string>();
                ExitCode versionExitCode = await ProcessHelper.ExecuteAsync(nuGetExePath, args, versionLogger)
                    .ConfigureAwait(false);

                if (!versionExitCode.IsSuccess)
                {
                    logger.Warning("NuGet version exit code was {VersionExitCode}", versionExitCode);
                    return;
                }

                const string nugetVersion = "NuGet Version: ";
                string versionLine =
                    standardOut.FirstOrDefault(
                        line => line.StartsWith(nugetVersion, StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrWhiteSpace(versionLine))
                {
                    logger.Warning("Could not ensure NuGet version, no version line in NuGet output");
                    return;
                }

                char majorNuGetVersion = versionLine.Substring(nugetVersion.Length).FirstOrDefault();

                if (majorNuGetVersion == '2')
                {
                    IEnumerable<string> updateSelfArgs = new List<string> { "update", "-self" };
                    ExitCode exitCode = await ProcessHelper.ExecuteAsync(nuGetExePath, updateSelfArgs, logger)
                        .ConfigureAwait(false);

                    if (!exitCode.IsSuccess)
                    {
                        logger.Warning("The NuGet version could not be determined, exit code {ExitCode}", exitCode);
                    }

                    return;
                }

                if (majorNuGetVersion != '3')
                {
                    logger.Warning(
                        "NuGet version could not be determined, major version starts with character {MajorNuGetVersion}",
                        majorNuGetVersion);
                    return;
                }

                logger.Verbose("NuGet major version is {MajorNuGetVersion}", majorNuGetVersion);
            }
            finally
            {
                if (versionLogger is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
