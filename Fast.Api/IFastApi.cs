using FastData.Core.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Threading.Tasks;

namespace Fast.Api
{
    public interface IFastApi
    {
        Task ContentAsync(HttpContext content, IFastRepository IFast, ICompositeViewEngine engine, bool IsResource=false, string dbFile = "db.json");
    }
}
