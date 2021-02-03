using FastData.Core.Repository;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fast.Api
{
    public interface IFastApi
    {
        Task ContentAsync(HttpContext content, IFastRepository IFast, bool IsResource=false, string dbFile = "db.json");
    }
}
