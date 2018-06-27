// Copyright 2018 Battelle Energy Alliance, LLC
// ALL RIGHTS RESERVED
using FakeADFSSecurity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Configuration;
using System.IdentityModel.Services;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FakeADFSMeta.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult FederationMetadata()
        {
            X509Certificate cert = CustomSecurityTokenService.GetCertificate();
            KeyInfoX509Data kid = new KeyInfoX509Data(cert, X509IncludeOption.WholeChain);
            var xml = kid.GetXml();
            string theKey = xml.InnerText;

            Uri uri = HttpContext.Request.Url;
            string adfsRoot = ConfigurationManager.AppSettings["FakeAdfsAt"];
            string url = $"{adfsRoot}/FederatedLogin/";

            string serviceDisplayName = "FakeADFS";

            StringBuilder ret = new StringBuilder();
            ret.Append($"<EntityDescriptor ID=\"_F38DBA4E-2F47-458D-BF6F-8A7EFB7C790A\" entityID=\"{url}\" xmlns=\"urn:oasis:names:tc:SAML:2.0:metadata\">");
            ret.Append($"<RoleDescriptor xsi:type=\"fed:SecurityTokenServiceType\" protocolSupportEnumeration=\"http://docs.oasis-open.org/ws-sx/ws-trust/200512 http://schemas.xmlsoap.org/ws/2005/02/trust http://docs.oasis-open.org/wsfed/federation/200706\" ServiceDisplayName=\"{serviceDisplayName}\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:fed=\"http://docs.oasis-open.org/wsfed/federation/200706\">");
            ret.Append($"<KeyDescriptor use=\"signing\"><KeyInfo xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><X509Data><X509Certificate>{theKey}</X509Certificate></X509Data></KeyInfo></KeyDescriptor>");
            ret.Append($"<fed:TokenTypesOffered><fed:TokenType Uri=\"urn:oasis:names:tc:SAML:1.0:assertion\" /></fed:TokenTypesOffered>");
            ret.Append($"<fed:PassiveRequestorEndpoint><EndpointReference xmlns=\"http://www.w3.org/2005/08/addressing\"><Address>{url}</Address></EndpointReference></fed:PassiveRequestorEndpoint>");
            ret.Append("</RoleDescriptor>");
            ret.Append("</EntityDescriptor>");

            return Content(ret.ToString());
        }
    }
}