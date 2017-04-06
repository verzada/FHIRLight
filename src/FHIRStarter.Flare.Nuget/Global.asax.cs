﻿using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using FhirStarter.Bonfire;
using FhirStarter.Bonfire.Spark.Engine.Extensions;
using log4net.Config;

// ReSharper disable once CheckNamespace
namespace FhirStarter.Flare
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            GlobalConfiguration.Configure(Configure);
          //  BundleConfig.RegisterBundles(BundleTable.Bundles);

            XmlConfigurator.Configure();
        }

        private void Configure(HttpConfiguration config)
        {
            config.AddFhir();
        }
    }
}
