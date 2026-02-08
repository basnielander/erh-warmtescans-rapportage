using Newtonsoft.Json.Serialization;
using System;
using System.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Routing;

namespace ERH.HeatScans.Reporting.Server.Framework
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Read CORS origins from configuration
            var corsOrigins = ConfigurationManager.AppSettings["CorsAllowedOrigins"] 
                ?? "https://localhost:49806,https://localhost:7209";

            // Enable CORS
            var cors = new EnableCorsAttribute(
                origins: corsOrigins,
                headers: "*",
                methods: "*")
            {
                SupportsCredentials = true
            };

            config.EnableCors(cors);

            // Read API root URL from configuration and extract the route prefix
            var apiRootUrl = ConfigurationManager.AppSettings["ApiRootUrl"] ?? "https://localhost:7209/api/";
            var apiRoutePrefix = ExtractRoutePrefix(apiRootUrl);

            // Web API routes with configurable prefix
            var constraintResolver = new DefaultInlineConstraintResolver();
            constraintResolver.ConstraintMap.Add("apiPrefix", typeof(ApiPrefixConstraint));
            config.MapHttpAttributeRoutes(new ApiPrefixRouteProvider(apiRoutePrefix));

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: apiRoutePrefix + "/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Configure JSON formatter
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            json.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;

            // Use camel case for JSON serialization
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }

        /// <summary>
        /// Extract the route prefix from the API root URL
        /// Example: "https://localhost:7209/api/" -> "api"
        /// </summary>
        private static string ExtractRoutePrefix(string apiRootUrl)
        {
            if (string.IsNullOrWhiteSpace(apiRootUrl))
            {
                return "api";
            }

            try
            {
                var uri = new Uri(apiRootUrl);
                var path = uri.AbsolutePath.Trim('/');
                return string.IsNullOrWhiteSpace(path) ? "api" : path;
            }
            catch
            {
                return "api";
            }
        }
    }

    /// <summary>
    /// Custom route provider that prepends the API prefix to all route prefixes
    /// </summary>
    public class ApiPrefixRouteProvider : DefaultDirectRouteProvider
    {
        private readonly string _apiPrefix;

        public ApiPrefixRouteProvider(string apiPrefix)
        {
            _apiPrefix = apiPrefix?.Trim('/') ?? "api";
        }

        protected override string GetRoutePrefix(System.Web.Http.Controllers.HttpControllerDescriptor controllerDescriptor)
        {
            var existingPrefix = base.GetRoutePrefix(controllerDescriptor);
            if (string.IsNullOrWhiteSpace(existingPrefix))
            {
                return _apiPrefix;
            }

            return $"{_apiPrefix}/{existingPrefix}";
        }
    }

    /// <summary>
    /// Constraint class for API prefix (currently not used but available for future use)
    /// </summary>
    public class ApiPrefixConstraint : IHttpRouteConstraint
    {
        public bool Match(System.Net.Http.HttpRequestMessage request, IHttpRoute route, string parameterName, System.Collections.Generic.IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            return true;
        }
    }
}
