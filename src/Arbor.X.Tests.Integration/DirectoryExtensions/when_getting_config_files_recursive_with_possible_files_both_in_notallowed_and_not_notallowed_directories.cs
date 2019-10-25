﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Arbor.Build.Core.IO;
using Machine.Specifications;
using Machine.Specifications.Model;

namespace Arbor.Build.Tests.Integration.DirectoryExtensions
{
    [Subject(typeof(Subject))]
    public class
        when_getting_config_files_recursive_with_possible_files_both_in_notallowed_and_not_notallowed_directories
    {
        static DirectoryInfo baseDir;
        static IReadOnlyCollection<string> files;

        Cleanup after = () => baseDir.DeleteIfExists();

        Establish context = () =>
        {
            baseDir =
                new DirectoryInfo(Path.Combine(Path.GetTempPath(),
                    $"{DefaultPaths.TempPathPrefix}_DirectoryExtensions_{Guid.NewGuid()}"));
            baseDir.EnsureExists();

            DirectoryInfo a = baseDir.CreateSubdirectory("A");
            DirectoryInfo bower = baseDir.CreateSubdirectory("bower_components");
            DirectoryInfo c = baseDir.CreateSubdirectory("C");

            DirectoryInfo e = c.CreateSubdirectory("e");

            DirectoryInfo nodeModules = bower.CreateSubdirectory("node_modules");

            using (File.Create(Path.Combine(nodeModules.FullName, "node.config")))
            {
            }

            using (File.Create(Path.Combine(nodeModules.FullName, "node.debug.config")))
            {
            }

            using (File.Create(Path.Combine(bower.FullName, "bower.config")))
            {
            }

            using (File.Create(Path.Combine(bower.FullName, "bower.debug.config")))
            {
            }

            using (File.Create(Path.Combine(a.FullName, "atest.config")))
            {
            }

            using (File.Create(Path.Combine(a.FullName, "atest.debug.config")))
            {
            }

            using (File.Create(Path.Combine(e.FullName, "etest.config")))
            {
            }

            using (File.Create(Path.Combine(e.FullName, "etest.debug.config")))
            {
            }
        };

        Because of = () =>
        {
            files = baseDir.GetFilesRecursive(new List<string> { ".config" },
                    DefaultPaths.DefaultPathLookupSpecification,
                    baseDir.FullName)
                .Select(s => s.Name)
                .ToList();
        };

        It should_contain_not_notallowed_files =
            () => files.ShouldContain("atest.config", "atest.debug.config", "etest.debug.config", "etest.config");

        It should_containt_correct_file_count = () => files.Count.ShouldEqual(4);

        It should_not_contain_notallowed_files =
            () => files.ShouldNotContain("bower.config", "bower.debug.config", "node.debug.config", "node.config");
    }
}
