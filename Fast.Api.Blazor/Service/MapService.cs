using static Fast.Api.Blazor.Pages.Xml;

namespace Fast.Api.Blazor.Service
{
    public class MapService : IMapService
    {
        public Task<Dictionary<string,object>> XmlSaveAsyn(object name,object xml)
        {
            var IFast = ServiceContext.Engine.Resolve<IFastRepository>();
            var result = new Dictionary<string, object>();
            try
            {
                if (string.IsNullOrEmpty(name.ToStr().ToLower().Replace(".xml", "")))
                {
                    result.Add("msg", "xml文件名填写不正确");
                    result.Add("Issuccess", false);
                }
                else
                {
                    using (var xmlWrite = System.IO.File.Create(name.ToStr()))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(xml.ToStr()));
                    }

                    if (IFast.CheckMap(name.ToStr()))
                    {
                        var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json", false);

                        if (!map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", name.ToStr().ToLower())))
                        {
                            var dic = new Dictionary<string, object>();
                            map.Path.Add(string.Format("map/{0}", name.ToStr()));
                            dic.Add("SqlMap", map);
                            var json = BaseJson.ModelToJson(dic);
                            System.IO.File.WriteAllText("map.json", json);
                        }

                        FastMap.InstanceMap();
                        result.Add("msg", "操作成功");
                        result.Add("Issuccess", true);
                    }
                    else
                    {
                        result.Add("msg", "操作失败");
                        result.Add("Issuccess", false);
                    }
                }
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.StackTrace, "xml");
                result.Add("msg", ex.Message);
                result.Add("Issuccess", false);
                return Task.FromResult(result);
            }
        }

        public Task<Dictionary<string, object>> XmlDelAsyn(object name)
        {
            var result = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(name.ToStr().ToLower().Replace(".xml", "")))
                result.Add("msg", "xml文件名填写不正确");
            else
            {
                System.IO.File.Delete(name.ToStr());
                var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json", false);
                if (map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", name.ToStr().ToLower())))
                {
                    var dic = new Dictionary<string, object>();
                    map.Path.Remove("map/" + name.ToStr());
                    dic.Add("SqlMap", map);
                    var json = BaseJson.ModelToJson(dic);
                    System.IO.File.WriteAllText("map.json", json);

                    FastMap.InstanceMap();
                }

                result.Add("msg", "操作成功");
            }
            return Task.FromResult(result);
        }
    }
}
