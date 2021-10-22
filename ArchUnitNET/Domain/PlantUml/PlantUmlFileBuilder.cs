﻿//  Copyright 2019 Florian Gather <florian.gather@tngtech.com>
// 	Copyright 2019 Fritz Brandhuber <fritz.brandhuber@tngtech.com>
// 	Copyright 2020 Pavel Fischer <rubbiroid@gmail.com>
// 
// 	SPDX-License-Identifier: Apache-2.0
// 

using System;
using System.Collections.Generic;
using System.Linq;

namespace ArchUnitNET.Domain.PlantUml
{
    public class PlantUmlFileBuilder
    {
        private IEnumerable<PlantUmlDependency> _dependencies;
        private IEnumerable<string> _objectsWithoutDependencies = Enumerable.Empty<string>();

        public List<string> Build()
        {
            var uml = new List<string> { "@startuml" };

            uml.AddRange(
                _objectsWithoutDependencies.Where(o => !string.IsNullOrEmpty(o)).Select(obj => "[" + obj + "]"));

            uml.AddRange(_dependencies
                .Where(dep => !string.IsNullOrEmpty(dep.Origin) && !string.IsNullOrEmpty(dep.Target))
                .Select(dependency => "[" + dependency.Origin + "] --> [" + dependency.Target + "]"));

            uml.Add("@enduml");
            return uml;
        }

        public PlantUmlFileBuilder WithDependenciesFrom(IEnumerable<PlantUmlDependency> dependencies,
            params string[] objectsWithoutDependencies)
        {
            _dependencies = dependencies;
            _objectsWithoutDependencies = objectsWithoutDependencies;
            return this;
        }

        public PlantUmlFileBuilder WithDependenciesFrom(IEnumerable<PlantUmlDependency> dependencies,
            IEnumerable<string> objectsWithoutDependencies)
        {
            _dependencies = dependencies;
            _objectsWithoutDependencies = objectsWithoutDependencies;
            return this;
        }

        public PlantUmlFileBuilder WithDependenciesFrom(IEnumerable<IType> types,
            bool includeDependenciesToOther = false)
        {
            return WithDependenciesFrom(types, type => type.FullName, type => type.FullName,
                includeDependenciesToOther);
        }

        public PlantUmlFileBuilder WithDependenciesFrom(IEnumerable<Slice> slices)
        {
            return WithDependenciesFrom(slices, slice => slice.Description,
                type =>
                    (from slice in slices where slice.Types.Contains(type) select slice.Description).FirstOrDefault());
        }

        public PlantUmlFileBuilder WithDependenciesFrom<T>(IEnumerable<T> objects,
            Func<T, string> identifierSelectorForObjects, Func<IType, string> identifierSelectorForTargetTypes,
            bool includeDependenciesToOther = false) where T : IHasDependencies
        {
            var processedDependencies = new List<PlantUmlDependency>();
            var objectsWithoutDependencies = new List<string>();
            var groupedObjects = objects.GroupBy(identifierSelectorForObjects);

            foreach (var group in groupedObjects)
            {
                var targets = group.SelectMany(o => o.Dependencies)
                    .Select(dep => identifierSelectorForTargetTypes(dep.Target))
                    .Distinct().Where(t => t != group.Key);

                if (!includeDependenciesToOther)
                {
                    targets = targets.Where(t => groupedObjects.Select(g => g.Key).Contains(t));
                }

                var plantUmlDependencies = targets.Select(t => new PlantUmlDependency(group.Key, t)).ToList();
                processedDependencies.AddRange(plantUmlDependencies);

                if (!plantUmlDependencies.Any())
                {
                    objectsWithoutDependencies.Add(group.Key);
                }
            }

            _dependencies = processedDependencies;
            _objectsWithoutDependencies = objectsWithoutDependencies;
            return this;
        }
    }
}