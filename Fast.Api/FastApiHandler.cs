using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FastData.Core.Repository;
using FastUntility.Core.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Fast.Api
{
    public class FastApiHandler
    {
        private readonly RequestDelegate next;
        private readonly ConfigOption option;

        public FastApiHandler(RequestDelegate request, ConfigOption _option = null)
        {
            next = request;
            option = _option;
        }

        public Task InvokeAsync(HttpContext context, IFastApi response, IFastRepository IFast)
        {
            var name = context.Request.Path.Value.ToStr().Substring(1, context.Request.Path.Value.ToStr().Length - 1).ToLower();

            if (option != null && option.FilterUrl.Exists(a => a.ToLower() == name) || name == "")
                return next(context);
            
            if (option != null && !option.IsAlone && (!IFast.IsExists(name) || IFast.MapDb(name).ToStr() == ""))
                return next(context);

            if (option != null)
                return response.ContentAsync(context, IFast, option.IsResource, option.dbFile);
            else
                return response.ContentAsync(context, IFast);
        }
    }
}
