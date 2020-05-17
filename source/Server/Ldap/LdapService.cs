using Octopus.Diagnostics;
using System;
using System.Threading;

namespace Octopus.Server.Extensibility.Authentication.Ldap
{
    public class LdapService : ILdapService
    {
        private readonly ILog log;
        private readonly ILdapObjectNameNormalizer objectNameNormalizer;
        private readonly ILdapContextProvider contextProvider;
        private readonly IUserPrincipalFinder _userPrincipalFinder;

        public LdapService(ILog log,
            ILdapObjectNameNormalizer objectNameNormalizer,
            ILdapContextProvider contextProvider,
            IUserPrincipalFinder userPrincipalFinder)
        {
            this.log = log;
            this.objectNameNormalizer = objectNameNormalizer;
            this.contextProvider = contextProvider;
            _userPrincipalFinder = userPrincipalFinder;
        }

        public UserValidationResult ValidateCredentials(string username, string password, CancellationToken cancellationToken)
        {
            log.Verbose($"Validating credentials provided for '{username}'...");

            objectNameNormalizer.NormalizeName(username, out username, out var domain);

            using (var context = contextProvider.GetContext())
            {
                var identityName = objectNameNormalizer.BuildUserName(username, domain);
                var principal = _userPrincipalFinder.FindByIdentity(context, identityName);

                if (principal == null)
                {
                    var searchedContext = domain ?? context.BaseDN;
                    log.Info($"A principal identifiable by '{identityName}' was not found in '{searchedContext}'");
                    if (username.Contains("@"))
                    {
                        return new UserValidationResult("Invalid username or password.  UPN format may not be supported for your domain configuration.");
                    }
                    return new UserValidationResult("Invalid username or password.");
                }

                try
                {
                    log.Verbose($"Calling Bind (\"{principal.DN}\")");
                    context.LdapConnection.Bind(principal.DN, password);
                }
                catch (Exception)
                {
                    log.Warn($"Principal '{principal.DN}' (Domain: '{domain}') could not be logged on via LDAP.");

                    return new UserValidationResult("Invalid username or password.");
                }

                log.Verbose($"Credentials for '{identityName}' validated, mapped to principal '{principal.SamAccountName}'");

                return new UserValidationResult(principal);
            }
        }

        public UserValidationResult FindByIdentity(string username)
        {
            objectNameNormalizer.NormalizeName(username, out username, out var domain);

            using (var context = contextProvider.GetContext())
            {
                var identityName = objectNameNormalizer.BuildUserName(username, domain);
                var principal = _userPrincipalFinder.FindByIdentity(context, identityName);

                if (principal == null)
                {
                    var searchedContext = domain ?? context.BaseDN;
                    return new UserValidationResult($"A principal identifiable by '{identityName}' was not found in '{searchedContext}'");
                }
                return new UserValidationResult(principal);
            }
        }
    }
}