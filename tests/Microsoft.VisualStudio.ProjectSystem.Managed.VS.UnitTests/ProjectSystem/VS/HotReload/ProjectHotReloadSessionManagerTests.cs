﻿// Licensed to the .NET Foundation under one or more agreements. The .NET Foundation licenses this file to you under the MIT license. See the LICENSE.md file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Debug;
using Xunit;

namespace Microsoft.VisualStudio.ProjectSystem.VS.HotReload
{
    public class ProjectHotReloadSessionManagerTests
    {
        [Fact]
        public async Task WhenActiveFrameworkMeetsRequirements_APendingSessionIsCreated()
        {
            var capabilities = new[] { "SupportsHotReload" };
            var propertyNamesAndValues = new Dictionary<string, string?>()
            {
                { "TargetFrameworkVersion", "v6.0" }
            };

            var activeConfiguredProject = CreateConfiguredProject(capabilities, propertyNamesAndValues);
            var manager = CreateHotReloadSessionManager(activeConfiguredProject);

            var environmentVariables = new Dictionary<string, string>();
            var sessionCreated = await manager.TryCreatePendingSessionAsync(environmentVariables);

            Assert.True(sessionCreated);
        }

        [Fact]
        public async Task WhenTheSupportsHotReloadCapabilityIsMissing_APendingSessionIsNotCreated()
        {
            var capabilities = new[] { "ARandomCapabilityUnrelatedToHotReload" };
            var propertyNamesAndValues = new Dictionary<string, string?>()
            {
                { "TargetFrameworkVersion", "v6.0" }
            };

            var activeConfiguredProject = CreateConfiguredProject(capabilities, propertyNamesAndValues);
            var manager = CreateHotReloadSessionManager(activeConfiguredProject);

            var environmentVariables = new Dictionary<string, string>();
            var sessionCreated = await manager.TryCreatePendingSessionAsync(environmentVariables);

            Assert.False(sessionCreated);
        }

        [Fact(Skip = "Bug: if TargetFrameworkVersion is not defined we still get a session")]
        public async Task WhenTheTargetFrameworkVersionIsNotDefined_APendingSessionIsNotCreated()
        {
            var capabilities = new[] { "SupportsHotReload" };
            var propertyNamesAndValues = new Dictionary<string, string?>()
            {
                { "ARandomProperty", "WithARandomValue" }
            };

            var activeConfiguredProject = CreateConfiguredProject(capabilities, propertyNamesAndValues);
            var manager = CreateHotReloadSessionManager(activeConfiguredProject);

            var environmentVariables = new Dictionary<string, string>();
            var sessionCreated = await manager.TryCreatePendingSessionAsync(environmentVariables);

            Assert.False(sessionCreated);
        }

        [Fact]
        public async Task WhenStartupHooksAreDisabled_APendingSessionIsNotCreated()
        {
            var capabilities = new[] { "SupportsHotReload" };
            var propertyNamesAndValues = new Dictionary<string, string?>()
            {
                { "TargetFrameworkVersion", "v6.0" },
                { "StartupHookSupport", "false" }
            };

            var activeConfiguredProject = CreateConfiguredProject(capabilities, propertyNamesAndValues);
            bool outputServiceCalled = false;
            Action<string> outputServiceCallback = message => outputServiceCalled = true;
            var manager = CreateHotReloadSessionManager(activeConfiguredProject, outputServiceCallback);

            var environmentVariables = new Dictionary<string, string>();
            var sessionCreated = await manager.TryCreatePendingSessionAsync(environmentVariables);

            Assert.False(sessionCreated);
            Assert.True(outputServiceCalled);
        }

        private static ProjectHotReloadSessionManager CreateHotReloadSessionManager(ConfiguredProject activeConfiguredProject, Action<string>? outputServiceCallback = null)
        {
            var activeDebugFrameworkServices = new IActiveDebugFrameworkServicesMock()
                .ImplementGetConfiguredProjectForActiveFrameworkAsync(activeConfiguredProject)
                .Object;

            var manager = new ProjectHotReloadSessionManager(
                UnconfiguredProjectFactory.Create(),
                IProjectThreadingServiceFactory.Create(),
                IProjectFaultHandlerServiceFactory.Create(),
                activeDebugFrameworkServices,
                new Lazy<IProjectHotReloadAgent>(() => IProjectHotReloadAgentFactory.Create()),
                new Lazy<IHotReloadDiagnosticOutputService>(() => IHotReloadDiagnosticOutputServiceFactory.Create(outputServiceCallback)));

            return manager;
        }

        private static ConfiguredProject CreateConfiguredProject(string[] capabilities, Dictionary<string, string?> propertyNamesAndValues)
        {
            return ConfiguredProjectFactory.Create(
                IProjectCapabilitiesScopeFactory.Create(capabilities),
                services: ConfiguredProjectServicesFactory.Create(
                    projectPropertiesProvider: IProjectPropertiesProviderFactory.Create(
                        commonProps: IProjectPropertiesFactory.CreateWithPropertiesAndValues(
                            propertyNamesAndValues))));
        }
    }
}