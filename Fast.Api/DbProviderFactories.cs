using FastData.Core.Base;
using FastData.Core.Model;
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
        public static DbProviderFactory GetFactory(string dbKey, bool IsResource = false, string dbFile = "db.json")
        {
            try
            {
                var config = new ConfigModel();

                if (IsResource)
                {
                    var projectName = Assembly.GetCallingAssembly().GetName().Name;
                    config = DataConfig.Get(dbKey, projectName, dbFile);
                }
                else
                    config = DataConfig.Get(dbKey);

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
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.Message + ex.StackTrace, "GetFactory");
                return null;
            }
        }
    }
}
