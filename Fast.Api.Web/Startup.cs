using FastData.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace Fast.Api.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //注册gbk
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("GB2312");

            //压缩
            services.AddResponseCompression();

            //注入
            services.AddTransient<IFastApi, FastApi>();
            
            //跨域
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
                template: "{controller=Home}/{action=index}/{id?}");
            });
        }
    }
}
