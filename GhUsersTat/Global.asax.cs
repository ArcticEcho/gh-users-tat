using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using GhUsersTat.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Extensions.Http;

using Serilog;

namespace GhUsersTat
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var serviceProvider = ConfigureServices();
            var resolver = new ServiceCollectionDependencyResolver(serviceProvider);
            DependencyResolver.SetResolver(resolver);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            var controllerList = typeof(MvcApplication).Assembly.GetTypes()
                .Where(type => type.BaseType == typeof(Controller))
                .ToList();

            foreach (var controller in controllerList)
            {
                services.AddTransient(controller);
            }

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .CreateLogger();

            services.AddLogging(x => x.AddSerilog(Log.Logger));
            services.AddSingleton<IGithubQueryService, GithubQueryService>();
            services.AddMemoryCache();

            services
                .AddHttpClient<IGithubQueryService, GithubQueryService>()
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(3, attempt - 1))));

            return services.BuildServiceProvider();
        }
    }
}
