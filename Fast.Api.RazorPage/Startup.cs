using System.Collections.Generic;
using System.Text;
using Fast.Api.RazorPage.Filter;
using FastUntility.Core.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

            services.AddFastApi(a => { a.IsResource = false; a.dbFile = "db.json"; a.mapFile = "map.json"; a.dbKey = "Api"; });

            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });

            services.AddRazorPages(o =>
            {
                o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                o.Conventions.AddPageRoute("/Help", "");
                o.RootDirectory = "/Pages";
            }).AddMvcOptions(options => { 
                options.Filters.Add(new CheckFilter());
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
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
            app.UseFastApiMiddleware(a =>
            {
                a.IsAlone = true;
                a.IsResource = false;
                a.FilterUrl.Add("help");
                a.FilterUrl.Add("xml");
                a.FilterUrl.Add("del");
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
