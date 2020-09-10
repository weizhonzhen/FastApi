using FastUntility.Core.Base;
using FastUntility.Core.Cache;
using System;
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
            var item = new ConfigModel();
            if (key != null)
                item = BaseConfig.GetListValue<ConfigModel>("DataConfig", "db.json").Find(a => a.Key.ToLower() == key.ToLower());
            else
                item = BaseConfig.GetListValue<ConfigModel>("DataConfig", "db.json")[0] ?? new ConfigModel();

            return item;
        }
    }
}
