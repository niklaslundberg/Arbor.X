﻿using System; using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.X.Core.BuildVariables;
using Arbor.X.Core.GenericExtensions;

using Arbor.X.Core.ProcessUtils;

namespace Arbor.X.Core.Tools.NuGet
{
    public class NuGetHelper
    {
        private readonly ILogger _logger;

        public NuGetHelper(ILogger logger)
        {
            _logger = logger;
        }

        public async Task<string> EnsureNuGetExeExistsAsync(string exeUri, CancellationToken cancellationToken)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string targetFile = Path.Combine(baseDir, "nuget.exe");

            const int MaxRetries = 6;

            var currentExePath = new FileInfo(targetFile);

            if (!File.Exists(targetFile))
            {
                string parentExePath = Path.Combine(currentExePath.Directory.Parent.FullName, currentExePath.Name);
                if (File.Exists(parentExePath))
                {
                    _logger.Information("Found NuGet in path '{ParentExePath}', skipping download", parentExePath);
                    return parentExePath;
                }

                _logger.Information("'{TargetFile}' does not exist, will try to download from nuget.org", targetFile);

                var uris = new List<string>();

                if (!string.IsNullOrWhiteSpace(exeUri) && Uri.TryCreate(exeUri, UriKind.Absolute, out Uri userUri))
                {
                    uris.Add(exeUri);
                }

                uris.Add("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe");
                uris.Add("https://nuget.org/nuget.exe");
                uris.Add("https://www.nuget.org/nuget.exe");

                for (int i = 0; i < MaxRetries; i++)
                {
                    try
                    {
                        string nugetExeUri = uris[i % uris.Count];

                        await DownloadNuGetExeAsync(baseDir, targetFile, nugetExeUri, cancellationToken).ConfigureAwait(false);

                        return targetFile;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Attempt {V}. Could not download nuget.exe. {Ex}", i + 1);
                    }

                    const int WaitTimeInSeconds = 1;

                    _logger.Information("Waiting {WaitTimeInSeconds} seconds to try again", WaitTimeInSeconds);

                    await Task.Delay(TimeSpan.FromSeconds(WaitTimeInSeconds), cancellationToken).ConfigureAwait(false);
                }
            }

            bool update = Environment.GetEnvironmentVariable(WellKnownVariables.NuGetVersionUpdatedEnabled)
                .TryParseBool(false);

            if (update)
            {
                try
                {
                    var arguments = new List<string> { "update", "-self" };
                    await ProcessHelper.ExecuteAsync(
                        targetFile,
                        arguments,
                        _logger,
                        addProcessNameAsLogCategory: true,
                        addProcessRunnerCategory: true,
                        cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.ToString());
                }
            }

            return targetFile;
        }

        private async Task DownloadNuGetExeAsync(
            string baseDir,
            string targetFile,
            string nugetExeUri,
            CancellationToken cancellationToken)
        {
            string tempFile = Path.Combine(baseDir, $"nuget.exe.{Guid.NewGuid()}.tmp");

            _logger.Verbose("Downloading {NugetExeUri} to {TempFile}", nugetExeUri, tempFile);
            try
            {
                using (var client = new HttpClient())
                {
                    using (Stream stream = await client.GetStreamAsync(nugetExeUri).ConfigureAwait(false))
                    {
                        using (var fs = new FileStream(tempFile, FileMode.Create))
                        {
                            await stream.CopyToAsync(fs, 4096, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
            }
            finally
            {
                if (File.Exists(tempFile) && new FileInfo(tempFile).Length > 0)
                {
                    File.Copy(tempFile, targetFile, true);
                    _logger.Verbose("Copied {TempFile} to {TargetFile}", tempFile, targetFile);
                    File.Delete(tempFile);
                    _logger.Verbose("Deleted temp file {TempFile}", tempFile);
                }
            }
        }
    }
}
