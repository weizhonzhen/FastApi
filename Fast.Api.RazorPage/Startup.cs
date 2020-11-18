using System.Collections.Generic;
using System.Text;
using Fast.Api.RazorPage.Filter;
using FastData.Core;
using FastUntility.Core.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FastData.Core.Repository;
using FastUntility.Core;
using FastRedis.Core.Repository;

namespace Fast.Api.RazorPage
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("GB2312");
            services.AddResponseCompression();
            services.AddFastData();
            services.AddFastApi();

            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
            
            FastMap.InstanceMap();

            services.AddMvc(options=> { options.Filters.Add(new CheckFilter()); }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddRazorPagesOptions(o => { 
                o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                o.RootDirectory = "/Pages";
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseExceptionHandler(error =>
            {
                error.Use(async (context, next) =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        BaseLog.SaveLog(contextFeature.Error.Message, "error");
                        context.Response.ContentType = "application/json;charset=utf-8";
                        context.Response.StatusCode = 200;
                        var result = new Dictionary<string, object>();
                        result.Add("success", false);
                        result.Add("msg", contextFeature.Error.Message);
                        await context.Response.WriteAsync(BaseJson.ModelToJson(result));
                    }
                });
            });

            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMiddleware<FastApiHandler>();
            app.UseMvc();
        }
    }
}
