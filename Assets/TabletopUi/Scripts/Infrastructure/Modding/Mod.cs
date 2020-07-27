
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OrbCreationExtensions;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Infrastructure.Modding
{
    public class Mod
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Author { get; set; }

        public Version Version { get; set; }

        public string Description { get; set; }

        public string DescriptionLong { get; set; }

        public List<Dependency> Dependencies { get; set; }

        public Dictionary<string, List<Hashtable>> Contents { get; set; }

        public Dictionary<string, Sprite> Images { get; set; }
        
        public bool Enabled { get; set; }

        private const string DependencyPattern = @"^\s*(\w+)(?:\s*(<=|<|>=|>|==)\s*([\d.]+))?\s*$";

        public Mod(
            string id,
            string name = null,
            string author = null,
            Version version = null,
            string description = null,
            string descriptionLong = null,
            List<Dependency> dependencies = null,
            Dictionary<string, List<Hashtable>> contents = null,
            Dictionary<string, Sprite> images = null)
        {
            Id = id;
            Name = name;
            Author = author;
            Version = version;
            Description = description;
            DescriptionLong = descriptionLong;
            Dependencies = dependencies ?? new List<Dependency>();
            Contents = contents ?? new Dictionary<string, List<Hashtable>>();
            Images = images ?? new Dictionary<string, Sprite>();
            Enabled = false;
        }

        public HashSet<string> FromManifest(Hashtable manifest)
        {
            var errors = new HashSet<string>();

            Name = manifest.GetStringOrLogError("name", errors);
            Author = manifest.GetStringOrLogError("author", errors);
            try
            {
                Version = new Version(manifest.GetStringOrLogError("version", errors));
            }
            catch
            {
                errors.Add("Invalid format for 'version'");
            }
            Description = manifest.GetStringOrLogError("description", errors);
            DescriptionLong = manifest.GetString("description");

            // Validate the dependencies
            var dependenciesData = manifest.GetArrayList("dependencies");
            if (dependenciesData != null)
            {
                Dependencies = new List<Dependency>();
                foreach (var dependencySpecification in dependenciesData)
                {
                    if (!(dependencySpecification is string))
                    {
                        errors.Add("Invalid dependency specification type, should be string");
                        continue;
                    }
                    var regex = new Regex(DependencyPattern);
                    var match = regex.Match((string) dependencySpecification);
                    if (match.Success)
                    {
                        var invalidDependency = false;
                        var dependency = new Dependency {ModId = match.Groups[1].Value};

                        // Validate the version, if specified
                        if (match.Groups.Count > 2
                            && match.Groups[2].Value.Length > 0 && match.Groups[3].Value.Length > 0)
                        {
                            switch (match.Groups[2].Value)
                            {
                                case "<":
                                    dependency.VersionOperator = DependencyOperator.LessThan;
                                    break;
                                case "<=":
                                    dependency.VersionOperator = DependencyOperator.LessThanOrEqual;
                                    break;
                                case ">":
                                    dependency.VersionOperator = DependencyOperator.GreaterThan;
                                    break;
                                case ">=":
                                    dependency.VersionOperator = DependencyOperator.GreaterThanOrEqual;
                                    break;
                                case "==":
                                    dependency.VersionOperator = DependencyOperator.Equal;
                                    break;
                                default:
                                    invalidDependency = true;
                                    errors.Add(
                                        "Invalid version operator '" + match.Groups[2].Value + "' for '" + dependency.ModId + "'");
                                    break;
                            }
                            try
                            {
                                dependency.Version = new Version(match.Groups[3].Value);
                            }
                            catch
                            {
                                errors.Add(
                                    "Invalid version number '" + match.Groups[3].Value + "' for '" + dependency.ModId + "'");
                                invalidDependency = true;
                            }
                        }

                        if (!invalidDependency)
                        {
                            Dependencies.Add(dependency);
                        }
                    }
                    else
                    {
                        errors.Add(
                            "Invalid dependency specification format '" + dependencySpecification + "'");
                    }
                }
            }
            else if (manifest.ContainsKey("dependencies"))
            {
                errors.Add("Invalid type for 'dependencies', should be array");
            }

            return errors;
        }

        public void AddContent(string category, ArrayList contents)
        {
            if (!Contents.ContainsKey(category))
            {
                Contents.Add(category, new List<Hashtable>());
            }
            var contentList = Contents[category];
            foreach (var content in contents)
            {
                contentList.Add((Hashtable) content);
            }
        }
    }
}
