using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fast.Api
{
    public class FastApiHandler
    {
        public FastApiHandler(RequestDelegate request) { }

        public Task InvokeAsync(HttpContext context, IFastApi response)
        {
            return response.ContentAsync(context);
        }
    }
}
