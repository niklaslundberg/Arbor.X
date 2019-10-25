using System;
using System.IO;
using Arbor.Build.Core.IO;
using Machine.Specifications;

namespace Arbor.Build.Tests.Integration.PathExtensions
{
    [Subject(typeof(Core.IO.PathExtensions))]
    public class when_checking_file_is_notallowed_with_root_dir
    {
        static bool isNotAllowed;

        static PathLookupSpecification specification;

        static string root;

        Cleanup after = () => new DirectoryInfo(root).DeleteIfExists();

        Establish context = () =>
        {
            root = $@"C:\Temp\root-{Guid.NewGuid()}";

            new DirectoryInfo(Path.Combine(root, "afolder")).EnsureExists();
            using (File.Create(Path.Combine(root, "afile.txt")))
            {
            }

            specification = DefaultPaths.DefaultPathLookupSpecification;
        };

        Because of =
            () => isNotAllowed = specification.IsFileExcluded($@"{root}\afile.txt", @"C:\Temp\root").Item1;

        It should_return_false = () => isNotAllowed.ShouldBeFalse();
    }
}
