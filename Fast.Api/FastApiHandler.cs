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
            if (context.Request.Path.Value.ToStr().Replace("/", "").ToLower() == "homeindex")
                return next(context);
            else
                return response.ContentAsync(context);
        }
    }
}
