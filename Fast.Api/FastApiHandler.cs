using FastUntility.Core.Base;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using FastData.Core.Repository;

namespace Fast.Api
{
    public class FastApiHandler
    {
        private readonly RequestDelegate next;

        public FastApiHandler(RequestDelegate request) { next = request; }

        public Task InvokeAsync(HttpContext context, IFastApi response,IFastRepository IFast)
        {
            var key = context.Request.Path.Value.ToStr().Substring(1, context.Request.Path.Value.ToStr().Length - 1).ToLower();
            if (key == "help" || key == "xml" || key == "del")
                return next(context);
            else
                return response.ContentAsync(context, IFast);
        }
    }
}
