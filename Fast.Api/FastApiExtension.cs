using Fast.Api;
using FastData.Core;
using FastData.Core.Model;
using Microsoft.AspNetCore.Builder;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastApiExtension
    {
        public static ConfigData config = new ConfigData();

        public static IServiceCollection AddFastApi(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            action(config);

            if (string.IsNullOrEmpty(config.dbKey))
                throw new Exception("config dbkey is not null");

            serviceCollection.AddSingleton<IFastApi, FastApi>();
            serviceCollection.AddFastData(action);
            return serviceCollection;
        }

        public static IServiceCollection AddFastApiGeneric(this IServiceCollection serviceCollection, Action<ConfigData> action, Action<ConfigRepository> repository)
        {
            action(config);

            if (string.IsNullOrEmpty(config.dbKey))
                throw new Exception("config dbkey is not null");

            serviceCollection.AddSingleton<IFastApi, FastApi>();
            serviceCollection.AddFastData(action);
            serviceCollection.AddFastDataGeneric(repository);

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
