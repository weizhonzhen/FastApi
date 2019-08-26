using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fast.Api
{
    public class FastApiHandler
    {
        public FastApiHandler(RequestDelegate request) { }

        public async Task InvokeAsync(HttpContext context, IFastApi response)
        {
            await response.ContentAsync(context).ConfigureAwait(false);
        }
    }
}
