﻿using System;
using System.Linq;
using Ldap.Integration.Tests.TestHelpers;
using Octopus.Server.Extensibility.Authentication.Extensions;
using Octopus.Server.Extensibility.Results;
using Xunit;

namespace Ldap.Integration.Tests
{
    public class IExternalGroupRetrieverTests
    {
        public class TheReadMethod
        {
            [Fact]
            internal void ReadsGroupsForAUserFromActiveDirectory()
            {
                // Arrange
                var user = new FakeUser("developer1");

                var expectedGroups = new[]
                {
                    "cn=DeveloperGroup1,ou=Groups,dc=mycompany,dc=local",
                    "cn=Developers,ou=Groups,dc=mycompany,dc=local",
                    "cn=Maintainers,ou=Groups,dc=mycompany,dc=local",
                };

                IExternalGroupRetriever fixture = FixtureHelper.CreateFixtureGroupRetriever(ConfigurationHelper.GetActiveDirectoryConfiguration());

                // Act
                var result = fixture.Read(user, new System.Threading.CancellationToken());

                // Assert
                if (result is FailureResultFromExtension)
                {
                    throw new Exception(string.Join("\n", (result as FailureResultFromExtension).Errors));
                }

                var groupResult = (ResultFromExtension<ExternalGroupResult>)result;

                Assert.Equal(expectedGroups.Select(x => x.ToLowerInvariant()).OrderBy(x => x), groupResult.Value.GroupIds.Select(x => x.ToLowerInvariant()).OrderBy(x => x));
            }

            [Fact]
            internal void ReadsGroupsForAUserFromOpenLDAP()
            {
                // Arrange
                var user = new FakeUser("developer1");

                var expectedGroups = new[]
                {
                    "cn=Maintainers,ou=Groups,dc=domain1,dc=local",
                    "cn=Developers,ou=Groups,dc=domain1,dc=local",
                    "cn=DeveloperGroup1,ou=Groups,dc=domain1,dc=local"
                };

                IExternalGroupRetriever fixture = FixtureHelper.CreateFixtureGroupRetriever(ConfigurationHelper.GetOpenLdapConfiguration());

                // Act
                var result = fixture.Read(user, new System.Threading.CancellationToken());

                // Assert
                if (result is FailureResultFromExtension)
                {
                    throw new Exception(string.Join("\n", (result as FailureResultFromExtension).Errors));
                }

                var groupResult = (ResultFromExtension<ExternalGroupResult>)result;

                Assert.Equal(expectedGroups.Select(x => x.ToLowerInvariant()).OrderBy(x => x), groupResult.Value.GroupIds.Select(x => x.ToLowerInvariant()).OrderBy(x => x));
            }
        }
    }
}