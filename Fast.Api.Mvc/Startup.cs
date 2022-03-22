using FastUntility.Core.Base;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace Fast.Api.Mvc
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("GB2312");

            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));
            services.AddResponseCompression();
            services.AddFastApi(a => { a.IsResource = false; a.dbFile = "db.json"; a.mapFile = "map.json"; a.dbKey = "Emr"; });

            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });

            services.AddMvc();
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
           
            app.UseFastApiMiddleware(a =>
            {
                a.IsResource = false;
                a.IsAlone = true;
                a.FilterUrl.Add("help");
                a.FilterUrl.Add("xml");
                a.FilterUrl.Add("del");
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                   name: "default",
                   pattern: "{action=Index}/{id?}");
            });

        }
    }
}
