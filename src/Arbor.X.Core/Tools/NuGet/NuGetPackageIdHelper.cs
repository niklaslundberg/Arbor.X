﻿using System;
using System.Collections.Generic;
using System.Linq;
using Arbor.Build.Core.Tools.Git;

namespace Arbor.Build.Core.Tools.NuGet
{
    public static class NuGetPackageIdHelper
    {
        public static string CreateNugetPackageId(
            string basePackageId,
            bool isReleaseBuild,
            string branchName,
            bool branchNameEnabled)
        {
            if (string.IsNullOrWhiteSpace(basePackageId))
            {
                throw new ArgumentNullException(nameof(basePackageId));
            }

            var branch = new BranchName(branchName);

            if (isReleaseBuild || !branch.IsFeatureBranch())
            {
                if (!branchNameEnabled)
                {
                    return basePackageId;
                }
            }

            return CreateNugetPackageIdWithBranchName(basePackageId, branch);
        }

        private static string CreateNugetPackageIdWithBranchName(string basePackageId, BranchName branch)
        {
            string normalizedBranchName = branch.Normalize();

            string nugetPackageId = $"{basePackageId}-{normalizedBranchName}";

            var invalidCharacters = new List<string> { "<", "@", ">", "|", "?", ":" };

            string trimmedName = invalidCharacters.Aggregate(
                nugetPackageId,
                (current, invalidCharacter) => current.Replace(invalidCharacter, string.Empty));

            return trimmedName;
        }
    }
}
