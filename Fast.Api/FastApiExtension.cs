using Fast.Api;
using FastData.Core;
using FastData.Core.Repository;
using FastUntility.Core;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastApiExtension
    {
        public static IServiceCollection AddFastApi(this IServiceCollection serviceCollection, Action<ConfigApi> action)
        {
            var config = new ConfigApi();
            action(config);

            if (string.IsNullOrEmpty(config.dbKey))
                throw new Exception("config dbkey is not null");

            var projectName = Assembly.GetCallingAssembly().GetName().Name;
            if (config.IsResource)
                FastMap.InstanceMapResource(config.dbKey, config.dbFile, config.mapFile, projectName);
            else
                FastMap.InstanceMap(config.dbKey, config.dbFile, config.mapFile);

            serviceCollection.AddTransient<IFastRepository, FastRepository>();
            serviceCollection.AddTransient<IFastApi, FastApi>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }

        public static IApplicationBuilder UseFastApiMiddleware(this IApplicationBuilder app, Action<ConfigOption> optionsAction)
        {
            var options = new ConfigOption();
            optionsAction(options);
            return app.UseMiddleware<FastApiHandler>(options);
        }
    }

    public class ConfigApi
    {
        public bool IsResource { get; set; }

        public string dbKey { get; set; }

        public string dbFile { get; set; } = "db.json";

        public string mapFile { get; set; } = "map.json";
    }

    public class ConfigOption
    {
        public bool IsAlone { get; set; }

        public bool IsResource { get; set; }

        public List<string> FilterUrl { get; set; } = new List<string>();

        public string dbFile { get; set; } = "db.json";
    }
}
