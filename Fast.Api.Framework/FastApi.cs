using System;
using FastData;
using FastData.Model;

namespace Fast.Api.Framework
{
    public class FastApi
    {
        public static void Init(Action<ConfigData> action)
        {
            FastMap.Init(action);
        }

        public static void InitGeneric(Action<ConfigRepository> action)
        {
            FastMap.InitGeneric(action);
        }
    }
}
