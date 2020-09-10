using FastUntility.Core.Base;
using FastUntility.Core.Cache;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace Fast.Api
{
    internal class DbProviderFactories : DbProviderFactory
    {
        /// <summary>
        /// 动态加载数据库工厂
        /// </summary>
        /// <param name="providerInvariantName"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public static DbProviderFactory GetFactory(string dbKey)
        {
            try
            {
                var config = GetConfig(dbKey);
                if (BaseCache.Exists(config.ProviderName))
                    return BaseCache.Get<object>(config.ProviderName) as DbProviderFactory;
                else
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == config.ProviderName);
                    if (assembly == null)
                        assembly = Assembly.Load(config.ProviderName);

                    var type = assembly.GetType(config.FactoryClient, false);
                    object instance = null;

                    if (type != null)
                        instance = Activator.CreateInstance(type);

                    BaseCache.Set<object>(config.ProviderName, instance);
                    return instance as DbProviderFactory;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取配置实体
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static ConfigModel GetConfig(string key = null)
        {
            var list = new List<ConfigModel>();
            var cacheKey = key == null ? "FastApi.Config" : string.Format("FastApi.Config.{0}", key);
            
            if (BaseCache.Exists(cacheKey))
                list = BaseCache.Get<List<ConfigModel>>( cacheKey);
            else
            {
                list = BaseConfig.GetListValue<ConfigModel>("DataConfig", "db.json");
                BaseCache.Set<List<ConfigModel>>(cacheKey, list);
            }

            if (string.IsNullOrEmpty(key))
                return list[0];
            else
                return list.Find(a => a.Key.ToLower() == key.ToLower());
        }
    }
}
