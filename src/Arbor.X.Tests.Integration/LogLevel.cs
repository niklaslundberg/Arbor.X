﻿using System;
using Arbor.Build.Core.BuildVariables;
using Arbor.Build.Core.Logging;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Arbor.Build.Tests.Integration
{
    public class InitializeLevelSwitch
    {
        [Fact]
        public void WhenUsingLogLevelEnvironmentVariableItShouldInitializeWithSuppliedLevel()
        {
            Environment.SetEnvironmentVariable(WellKnownVariables.LogLevel, "Debug");
            LoggingLevelSwitch loggingLevelSwitch = LogLevelHelper.GetLevelSwitch(null);
            Environment.SetEnvironmentVariable(WellKnownVariables.LogLevel, null);

            Assert.NotNull(loggingLevelSwitch);
            Assert.Equal(LogEventLevel.Debug, loggingLevelSwitch.MinimumLevel);
        }
    }
}
