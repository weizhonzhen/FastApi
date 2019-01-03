using Microsoft.AspNetCore.Http;

namespace Fast.Api
{
    public interface IFastApi
    {
        void Content(HttpContext content);
    }
}
