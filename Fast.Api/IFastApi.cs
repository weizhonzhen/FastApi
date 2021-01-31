using FastData.Core.Repository;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fast.Api
{
    public interface IFastApi
    {
        Task ContentAsync(HttpContext content, IFastRepository IFast, RequestDelegate next, OptionModel option);
    }
}
