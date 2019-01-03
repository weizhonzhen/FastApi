using FastData.Core.Model;
using FastUntility.Core.Base;
using System;
using System.Data.Common;
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
                var assembly = Assembly.Load(config.ProviderName);
                var type = assembly.GetType(config.FactoryClient, false);
                object instance = null;

                if (type != null)
                    instance = type.InvokeMember("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty, null, type, null);

                return instance as DbProviderFactory;
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
