﻿using System.Linq;
using System.Threading;
using Ldap.Integration.Tests.TestHelpers;
using Octopus.Data.Model.User;
using Octopus.Server.Extensibility.Authentication.Extensions;
using Octopus.Server.Extensibility.Results;
using Xunit;

namespace Ldap.Integration.Tests
{
    public class ISupportsAutoUserCreationFromPrincipalTests
    {
        public class TheGetOrCreateUserMethod
        {
            [Fact]
            internal void CreatesAUserFromActiveDirectory()
            {
                // Arrange
                var userName = "developer1";

                ISupportsAutoUserCreationFromPrincipal fixture = FixtureHelper.CreateLdapUserCreationFromPrincipal(ConfigurationHelper.GetActiveDirectoryConfiguration(), userName);

                // Act
                var result = fixture.GetOrCreateUser(new FakePrincipal(userName), new CancellationToken());

                // Assert
                ExtensionResultHelper.AssertSuccesfulExtensionResult(result);
                var createdUser = ((ResultFromExtension<IUser>)result).Value;

                Assert.Equal("developer1@mycompany.local", createdUser.Username);
                Assert.Equal("Developer User 1", createdUser.DisplayName);
                Assert.Equal("developer1@mycompany.local", createdUser.EmailAddress);
                Assert.Equal("developer1", createdUser.Identities.First().Claims["uan"].Value);
                Assert.Equal("developer1@mycompany.local", createdUser.Identities.First().Claims["upn"].Value);
                Assert.Equal("developer1@mycompany.local", createdUser.Identities.First().Claims["email"].Value);
                Assert.Equal("Developer User 1", createdUser.Identities.First().Claims["dn"].Value);
            }

            [Fact]
            internal void CreatesAUserFromOpenLDAP()
            {
                // Arrange
                var userName = "developer1";

                ISupportsAutoUserCreationFromPrincipal fixture = FixtureHelper.CreateLdapUserCreationFromPrincipal(ConfigurationHelper.GetOpenLdapConfiguration(), userName);

                // Act
                var result = fixture.GetOrCreateUser(new FakePrincipal(userName), new CancellationToken());

                // Assert
                ExtensionResultHelper.AssertSuccesfulExtensionResult(result);
                var createdUser = ((ResultFromExtension<IUser>)result).Value;

                Assert.Equal("developer1", createdUser.Username);
                Assert.Equal("Developer User 1", createdUser.DisplayName);
                Assert.Equal("developer1@gmail.com", createdUser.EmailAddress);
                Assert.Equal("developer1", createdUser.Identities.First().Claims["uan"].Value);
                Assert.Equal("developer1", createdUser.Identities.First().Claims["upn"].Value);
                Assert.Equal("developer1@gmail.com", createdUser.Identities.First().Claims["email"].Value);
                Assert.Equal("Developer User 1", createdUser.Identities.First().Claims["dn"].Value);
            }
        }
    }
}