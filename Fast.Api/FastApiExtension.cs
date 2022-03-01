﻿using Fast.Api;
using FastData.Core;
using FastData.Core.Repository;
using FastUntility.Core;
using Microsoft.AspNetCore.Builder;
using System;
using System.Reflection;
using System.Linq;

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

            serviceCollection.AddSingleton<IFastRepository, FastRepository>();
            serviceCollection.AddSingleton<IFastApi, FastApi>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));

            Assembly.GetCallingAssembly().GetReferencedAssemblies().ToList().ForEach(a => {
                try
                {
                    if (!AppDomain.CurrentDomain.GetAssemblies().ToList().Exists(b => b.GetName().Name == a.Name))
                        Assembly.Load(a.Name);
                }
                catch (Exception ex) { }
            });

            if (config.IsResource)
                FastMap.InstanceMapResource(config.dbKey, config.dbFile, config.mapFile, Assembly.GetCallingAssembly().GetName().Name);
            else
                FastMap.InstanceMap(config.dbKey, config.dbFile, config.mapFile);

            return serviceCollection;
        }

        public static IApplicationBuilder UseFastApiMiddleware(this IApplicationBuilder app, Action<ConfigOption> optionsAction)
        {
            var options = new ConfigOption();
            optionsAction(options);
            return app.UseMiddleware<FastApiHandler>(options);
        }
    }
}
