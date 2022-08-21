[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Fast.Api.Framework.RegisterModule), "Start")]

namespace Fast.Api.Framework
{
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    public static class RegisterModule
    {
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(FastApiModule));
        }
    }
}
