﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using GhUsersTat.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            DependencyResolver.SetResolver(new ServiceCollectionDependencyResolver(serviceProvider));
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
            services.AddHttpClient();
            services.AddSingleton<IGithubQueryService, GithubQueryService>();

            return services.BuildServiceProvider();
        }
    }
}
