using FastData.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddTransient<IFastApi, FastApi>();
            
            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });

            services.AddMvc();

            FastMap.InstanceMap();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseStaticFiles();
            app.UseMiddleware<FastApiHandler>();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                name: "default",
                template: "{action=index}/{id?}");
            });
        }
    }
}
