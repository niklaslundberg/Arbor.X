﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Arbor.Build.Core.BuildVariables;
using Arbor.Build.Core.IO;
using Arbor.Build.Core.Tools.Cleanup;
using JetBrains.Annotations;
using Serilog;

namespace Arbor.Build.Core.Tools.Testing
{
    [UsedImplicitly]
    public class MSpecVariableProvider : IVariableProvider
    {
        public int Order => VariableProviderOrder.Ignored;

        public Task<ImmutableArray<IVariable>> GetBuildVariablesAsync(
            ILogger logger,
            IReadOnlyCollection<IVariable> buildVariables,
            CancellationToken cancellationToken)
        {
            string reportPath = buildVariables.Require(WellKnownVariables.ReportPath).ThrowIfEmptyValue().Value;

            var reportDirectory = new DirectoryInfo(reportPath);

            var testReportPathDirectory = new DirectoryInfo(Path.Combine(
                reportDirectory.FullName,
                MachineSpecificationsConstants.MachineSpecificationsName));

            testReportPathDirectory.EnsureExists();

            var environmentVariables = new IVariable[]
            {
                new BuildVariable(
                    WellKnownVariables.ExternalTools_MSpec_ReportPath,
                    testReportPathDirectory.FullName)
            };

            return Task.FromResult(environmentVariables.ToImmutableArray());
        }
    }
}
