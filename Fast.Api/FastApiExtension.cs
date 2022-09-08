using Fast.Api;
using FastData.Core.Model;
using Microsoft.AspNetCore.Builder;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastApiExtension
    {
        public static ConfigData config = new ConfigData();

        public static IServiceCollection AddFastApi(this IServiceCollection serviceCollection, Action<ConfigData> action)
        {
            action(config);
            
            if (string.IsNullOrEmpty(config.DbKey))
                throw new Exception("config dbkey is not null");

            var Current= Assembly.GetCallingAssembly();

            serviceCollection.AddSingleton<IFastApi, FastApi>();
            serviceCollection.AddFastData(a => {
                a.Aop = config.Aop;
                a.IsCodeFirst = config.IsCodeFirst;
                a.MapFile= config.MapFile;
                a.DbFile= config.DbFile;
                a.DbKey=config.DbKey;
                a.IsResource= config.IsResource;
                a.Current = Current;
                a.NamespaceCodeFirst = config.NamespaceCodeFirst;
                a.NamespaceService = config.NamespaceService;
                a.NamespaceProperties = config.NamespaceProperties;           
            });
            return serviceCollection;
        }

        public static IServiceCollection AddFastApiGeneric(this IServiceCollection serviceCollection, Action<ConfigData> action, Action<ConfigRepository> repository)
        {
            action(config);

            if (string.IsNullOrEmpty(config.DbKey))
                throw new Exception("config dbkey is not null");

            var Current = Assembly.GetCallingAssembly();

            serviceCollection.AddSingleton<IFastApi, FastApi>();
            serviceCollection.AddFastData(a => {
                a.Aop = config.Aop;
                a.IsCodeFirst = config.IsCodeFirst;
                a.MapFile = config.MapFile;
                a.DbFile = config.DbFile;
                a.DbKey = config.DbKey;
                a.IsResource = config.IsResource;
                a.Current = Current;
                a.NamespaceCodeFirst = config.NamespaceCodeFirst;
                a.NamespaceService = config.NamespaceService;
                a.NamespaceProperties = config.NamespaceProperties;
            });
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
