using System.Text;
using Fast.Api.RazorPage.Filter;
using FastData.Core;
using Microsoft.AspNetCore.Builder;
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encoding = Encoding.GetEncoding("GB2312");

            services.AddResponseCompression();

            services.AddTransient<IFastApi, FastApi>();

            services.AddCors(options =>
            {
                options.AddPolicy("any", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); });
            });
            
            FastMap.InstanceMap();

            services.AddMvc(options=> { options.Filters.Add(new CheckFilter()); }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddRazorPagesOptions(o => { o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute()); });
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
                        context.Response.ContentType = "application/json";
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync(contextFeature.Error.Message);
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
