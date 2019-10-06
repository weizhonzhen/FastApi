using FastUntility.Core.Base;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fast.Api
{
    public class FastApiHandler
    {
        private readonly RequestDelegate next;

        public FastApiHandler(RequestDelegate request) { next = request; }

        public Task InvokeAsync(HttpContext context, IFastApi response)
        {
            var key = context.Request.Path.Value.ToStr().Replace("/", "").ToLower();
            if (key == "help" || key == "editxml")
                return next(context);
            else
                return response.ContentAsync(context);
        }
    }
}
