// Copyright 2018 Battelle Energy Alliance, LLC
// ALL RIGHTS RESERVED
using System;
using System.IdentityModel;
using System.IdentityModel.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace FakeADFSSecurity
{
    public class CustomSecurityTokenService : SecurityTokenService
    {
        static readonly string[] supportedWebApps = { };

        public CustomSecurityTokenService(SecurityTokenServiceConfiguration securityTokenServiceConfiguration)
            : base(securityTokenServiceConfiguration)
        { }

        static void ValidateAppliesTo(EndpointReference appliesTo)
        {
            if (supportedWebApps == null || supportedWebApps.Length == 0)
                return;

            var validAppliesTo = supportedWebApps.Any(x => appliesTo.Uri.Equals(x));

            if (!validAppliesTo)
                throw new InvalidRequestException(String.Format("The 'appliesTo' address '{0}' is not valid.", appliesTo.Uri.OriginalString));
        }

        protected override Scope GetScope(ClaimsPrincipal principal, RequestSecurityToken request)
        {
            ValidateAppliesTo(request.AppliesTo);

            var scope = new Scope(request.AppliesTo.Uri.OriginalString, SecurityTokenServiceConfiguration.SigningCredentials);

            scope.TokenEncryptionRequired = false;
            
            if (!string.IsNullOrEmpty(request.ReplyTo))
                scope.ReplyToAddress = request.ReplyTo;
            else
                scope.ReplyToAddress = scope.AppliesToAddress;

            return scope;
        }

        protected override ClaimsIdentity GetOutputClaimsIdentity(ClaimsPrincipal principal, RequestSecurityToken request, Scope scope)
        {
            string upn = principal.Identity.Name;
            Claim[] claims = new Claim[]
            {
                new Claim("http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider", "FakeADFS"),
                new Claim(System.IdentityModel.Claims.ClaimTypes.Upn, principal.Identity.Name),
                new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/windowsaccountname", principal.Identity.Name),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims);

            return identity;
        }
        
        public static X509Certificate GetCertificate()
        {
            return new X509Certificate($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\Certificates\FakeADFSCert.pfx", "TestingOnly1");
        }

        public static X509Certificate2 GetCertificate2()
        {
            return new X509Certificate2($@"{AppDomain.CurrentDomain.BaseDirectory}\bin\Certificates\FakeADFSCert.pfx", "TestingOnly1");
        }
    }
}
