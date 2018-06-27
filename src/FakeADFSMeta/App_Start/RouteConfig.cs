// Copyright 2018 Battelle Energy Alliance, LLC
// ALL RIGHTS RESERVED
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FakeADFSMeta
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "FederationMetadata",
                url: "FederationMetadata/2007-06/FederationMetadata.xml",
                defaults: new { controller = "Home", action = "FederationMetadata" });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
