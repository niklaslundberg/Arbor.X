﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Arbor.X.Core.Tools.MSBuild
{
    public class MSBuildProject
    {
        public static bool IsNetSdkProject([NotNull] FileInfo projectFile)
        {
            if (projectFile == null)
            {
                throw new ArgumentNullException(nameof(projectFile));
            }

            if (projectFile.FullName.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                throw new InvalidOperationException("The temp path contains invalid characters");
            }

            if (projectFile.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new InvalidOperationException("The temp path contains invalid characters");
            }

            return File.ReadLines(projectFile.FullName)
                .Any(line => line.IndexOf("Microsoft.NET.Sdk", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private MSBuildProject(
            IReadOnlyList<MSBuildPropertyGroup> propertyGroups,
            string fileName,
            string projectName,
            string projectDirectory,
            ImmutableArray<ProjectType> projectTypes,
            Guid? projectId)
        {
            PropertyGroups = propertyGroups.ToImmutableArray();
            FileName = fileName;
            ProjectName = projectName;
            ProjectDirectory = projectDirectory;
            ProjectTypes = projectTypes;
            ProjectId = projectId;
        }

        public ImmutableArray<MSBuildPropertyGroup> PropertyGroups { get; }

        public string FileName { get; }

        public string ProjectName { get; }

        public string ProjectDirectory { get; }

        public ImmutableArray<ProjectType> ProjectTypes { get; }

        public Guid? ProjectId { get; }

        public static MSBuildProject LoadFrom(string projectFileFullName)
        {
            using (var fs = new FileStream(projectFileFullName, FileMode.Open, FileAccess.Read))
            {
                var msbuildPropertyGroups = new List<MSBuildPropertyGroup>();

                Guid? projectId = default;

                XDocument document = XDocument.Load(fs);

                const string projectElementName = "Project";

                XElement project = document.Elements().SingleOrDefault(element => element.Name.LocalName.Equals(projectElementName, StringComparison.Ordinal));

                if (project is null)
                {
                    throw new InvalidOperationException($"Could not find element <{projectElementName}> in file '{projectFileFullName}'");
                }

                ImmutableArray<XElement> propertyGroups = project.Elements("PropertyGroup").ToImmutableArray();

                XElement idElement = propertyGroups.Elements("ProjectGuid").FirstOrDefault();

                if (Guid.TryParse(idElement?.Value, out Guid id))
                {
                    projectId = id;
                }

                foreach (XElement propertyGroup in propertyGroups)
                {
                    ImmutableArray<MSBuildProperty> msBuildProperties = propertyGroup?.Elements()
                                                                            .Select(p =>
                                                                                new MSBuildProperty(p.Name.LocalName,
                                                                                    p.Value))
                                                                            .ToImmutableArray()
                                                                        ?? ImmutableArray<MSBuildProperty>.Empty;

                    msbuildPropertyGroups.Add(new MSBuildPropertyGroup(msBuildProperties));
                }

                string name = Path.GetFileNameWithoutExtension(projectFileFullName);

                var file = new FileInfo(projectFileFullName);

                ImmutableArray<ProjectType> projectTypes = propertyGroups
                                                               .Elements("ProjectTypeGuids")
                                                               .FirstOrDefault()?.Value.Split(';')
                                                               .Select(Guid.Parse)
                                                               .Select(guid => new ProjectType(guid))
                                                               .ToImmutableArray()
                                                           ?? ImmutableArray<ProjectType>.Empty;

                return new MSBuildProject(msbuildPropertyGroups,
                    projectFileFullName,
                    name,
                    file.Directory?.FullName,
                    projectTypes,
                    projectId);
            }
        }
    }
}
