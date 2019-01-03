using Fast.Api;
using FastData.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace TestApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression();
            services.AddTransient<IFastApi, FastApi>();
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
            
            FastMap.InstanceProperties("Fast.Api.DataModel", "Fast.Api.dll");
            FastMap.InstanceTable("Fast.Api.DataModel", "Fast.Api.dll");
            FastMap.InstanceMap();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<FastApiHandler>();
        }
    }
}
