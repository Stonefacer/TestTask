using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.Remove(config.Formatters.XmlFormatter); // remove xml formatter so we could use json formatter by default

            // Web API routes
            config.MapHttpAttributeRoutes();

            // for LogController
            config.Routes.MapHttpRoute(
                name: "LogApi",
                routeTemplate: "api/log/{offset}/{limit}",
                defaults: new
                {
                    controller = "Log",
                    limit = RouteParameter.Optional
                }
            );

            // other controllers
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{count}",
                defaults: new {
                    count = RouteParameter.Optional
                }
            );
        }
    }
}
