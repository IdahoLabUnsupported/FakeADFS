// Copyright 2018 Battelle Energy Alliance, LLC
// ALL RIGHTS RESERVED
using FakeADFSSecurity;
using System;
using System.IdentityModel.Configuration;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Web.Mvc;

namespace FakeADFS.Controllers
{
    public class FederatedLoginController : Controller
    {
        public const string Action = "wa";
        public const string SignIn = "wsignin1.0";
        public const string SignOut = "wsignout1.0";

        public ActionResult Index()
        {
            var action = Request.QueryString[Action];

            if (action == SignIn)
            {
                var formData = ProcessSignIn(Request.Url, (ClaimsPrincipal)User);
                return new ContentResult() { Content = formData, ContentType = "text/html" };
            }
            else if (action == SignOut)
            {
                throw new NotImplementedException();
            }

            throw new Exception($"Unknown action '{action}'");
        }

        public static string ProcessSignIn(Uri url, ClaimsPrincipal user)
        {
            var requestMessage = (SignInRequestMessage)WSFederationMessage.CreateFromUri(url);
            var signingCredentials = new X509SigningCredentials(CustomSecurityTokenService.GetCertificate2());
            var config = new SecurityTokenServiceConfiguration($"{url.Scheme}://{url.Authority}/FederatedLogin/", signingCredentials);
            var sts = new CustomSecurityTokenService(config);
            var responseMessage = FederatedPassiveSecurityTokenServiceOperations.ProcessSignInRequest(requestMessage, user, sts);
            return responseMessage.WriteFormPost();
        }
    }
}