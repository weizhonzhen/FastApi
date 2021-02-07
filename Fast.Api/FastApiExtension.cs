using Fast.Api;
using FastData.Core;
using FastData.Core.Repository;
using FastUntility.Core;
using Microsoft.AspNetCore.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastApiExtension
    {
        public static IServiceCollection AddFastApi(this IServiceCollection serviceCollection, ConfigModel config)
        {
            if (config.IsResource)
                FastMap.InstanceMapResource(config.dbKey, config.dbFile, config.mapFile);
            else
                FastMap.InstanceMap(config.dbKey, config.dbFile, config.mapFile);

            serviceCollection.AddTransient<IFastRepository, FastRepository>();
            serviceCollection.AddTransient<IFastApi, FastApi>();
            ServiceContext.Init(new ServiceEngine(serviceCollection.BuildServiceProvider()));
            return serviceCollection;
        }

        public static IApplicationBuilder UseFastApiMiddleware(this IApplicationBuilder app, Action<OptionModel> optionsAction)
        {
            var options = new OptionModel();
            optionsAction(options);
            return app.UseMiddleware<FastApiHandler>(options);
        }
    }
}
