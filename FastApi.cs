using Fast.Api.DataModel;
using FastData.Core;
using FastData.Core.Context;
using FastUntility.Core.Base;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Fast.Api
{
    public class FastApi : IFastApi
    {
        //接口数据库
        public static string DbApi = "Api";

        public void Content(HttpContext context)
        {
            var urlParam = GetUrlParam(context);
            var isSuccess = true;
            var dic = new Dictionary<string, object>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            context.Response.ContentType = "application/Json";
            var key = context.Request.Path.Value.ToStr().Replace("/", "").ToUpper();

            if (string.IsNullOrEmpty(key) || key == "favicon.ico")
                isSuccess = false;

            using (var db = new DataContext(DbApi))
            {
                if (isSuccess && FastRead.Query<ApiData>(a => a.Key.ToUpper() == key).ToCount(db) > 0)
                {
                    var info = FastRead.Query<ApiData>(a => a.Key.ToUpper() == key).ToItem<ApiData>(db);

                    if (info.IsAnonymous == 0)
                    {
                        if (!CheckToken(context, info, db, urlParam))
                        {
                            isSuccess = false;
                            dic.Add("error", "接口无权访问");
                        }
                    }

                    if (isSuccess)
                    {
                        var data = new List<Dictionary<string, object>>();
                        var mapList = FastRead.Query<ApiMap>(a => a.Key.ToUpper() == key).OrderBy<ApiMap>(a => new { a.OderBy }, false).ToList<ApiMap>(db);

                        foreach (var map in mapList)
                        {
                            var param = new List<DbParameter>();
                            var paramList = FastRead.Query<ApiMapParam>(a => a.MapId.ToUpper() == map.MapId.ToUpper()).OrderBy<ApiMapParam>(a => new { a.OderBy }, false).ToList<ApiMapParam>(db);

                            foreach (var item in paramList)
                            {
                                var tempParam = DbProviderFactories.GetFactory(DbApi).CreateParameter();
                                tempParam.ParameterName = item.ParamName;
                                tempParam.Value = GetDbPrarm(item.Source, data.FirstOrDefault(), item.ParamName, urlParam);
                                param.Add(tempParam);
                            }

                            if (map.IsWrite == 1)
                                isSuccess = FastMap.Write(map.MapId, param.ToArray(), null, map.DbKey).IsSuccess;
                            else
                            {
                                isSuccess = true;
                                data = FastMap.Query(map.MapId, param.ToArray(), null, map.DbKey);

                                //子map
                                if (FastRead.Query<ApiMapLeaf>(a => a.MapId.ToUpper() == map.MapId.ToUpper()).ToCount(db) > 0)
                                    data = GetLeafMap(db, map, data, urlParam);
                            }
                        }

                        dic.Add("data", data);
                    }
                }
                else
                {
                    isSuccess = false;
                    dic.Add("error", "接口不存在");
                }

                Log(context, stopwatch, key, db, urlParam);
                dic.Add("isSuccess", isSuccess);
                context.Response.StatusCode = 200;
                context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8);
            }
        }

        #region 子map
        /// <summary>
        /// 子map
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> GetLeafMap(DataContext db, ApiMap map, List<Dictionary<string, object>> data, string urlParam)
        {
            var param = new List<DbParameter>();
            var leafList = FastRead.Query<ApiMapLeaf>(a => a.MapId.ToUpper() == map.MapId.ToUpper()).OrderBy<ApiMapLeaf>(a => new { a.OderBy }, false).ToList<ApiMapLeaf>(db);
            var tempData = data;

            foreach (var leafInfo in leafList)
            {
                foreach (var item in tempData)
                {
                    var leafParam = FastRead.Query<ApiMapLeafParam>(a => a.LeafMapId.ToUpper() == leafInfo.LeafMapId.ToUpper()).ToList<ApiMapParam>(db);

                    param.Clear();
                    foreach (var leafItem in leafParam)
                    {
                        var tempParam = DbProviderFactories.GetFactory(DbApi).CreateParameter();
                        tempParam.ParameterName = leafItem.ParamName;
                        tempParam.Value = GetDbPrarm(leafItem.Source, item, leafItem.ParamName, urlParam);
                        param.Add(tempParam);
                    }

                    var tempDic = FastMap.Query(map.MapId, param.ToArray(), null, map.DbKey).FirstOrDefault();

                    //参数默认值
                    if (leafInfo.ResultParam.IndexOf('|') > 0)
                    {
                        data.Remove(item);
                        foreach (var temp in leafInfo.ResultParam.Split('|'))
                        {
                            item.Add(temp, tempDic.GetValue(temp));
                        }
                        data.Add(item);
                    }
                    else if (!string.IsNullOrEmpty(leafInfo.ResultParam))
                    {
                        data.Remove(item);
                        item.Add(leafInfo.ResultParam, tempDic.GetValue(leafInfo.ResultParam));
                        data.Add(item);
                    }
                }
            }
            return data;
        }
        #endregion

        #region 验证权限访问
        /// <summary>
        /// 验证权限访问
        /// </summary>
        /// <param name="context"></param>
        /// <param name="api"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool CheckToken(HttpContext context, ApiData api, DataContext db, string urlParam)
        {
            var AppSecret = GetUrlParam(urlParam, "AppSecret").ToUpper();

            if (FastRead.Query<ApiToken>(a => a.AppSecret.ToUpper() == AppSecret).ToCount(db) > 0)
            {
                var info = FastRead.Query<ApiToken>(a => a.AppSecret.ToUpper() == AppSecret).ToItem<ApiToken>(db);

                if (info.IsBindIp == 1 && GetClientIp(context) != info.IpAddress)
                    return false;

                if (info.Key.IndexOf(',') > 0)
                {
                    foreach (var temp in info.Key.Split(','))
                    {
                        if (temp.ToLower() == api.Key.ToLower())
                            return true;
                    }
                }
                else if (api.Key.ToLower() == info.Key.ToLower())
                    return true;
            }

            return false;
        }
        #endregion

        #region 获取map参数值 
        /// <summary>
        /// 获取map参数值
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static object GetDbPrarm(decimal source, Dictionary<string, object> dic, string paramName, string urlParam)
        {
            //url
            if (source == 1)
                return urlParam;

            //map
            if (source == 2)
                return dic.GetValue(paramName);

            return "";
        }
        #endregion

        #region 获取参数
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string GetUrlParam(HttpContext context)
        {
            var content = HttpUtility.UrlDecode(new StreamReader(context.Request.Body).ReadToEnd());
            var param = HttpUtility.UrlDecode(context.Request.QueryString.Value);

            if (string.IsNullOrEmpty(param))
                param = content;

            if (param.Substring(0, 1) == "?")
                param = param.Substring(1, param.Length - 1);

            return param;
        }
        #endregion

        #region 获取参数
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static string GetUrlParam(string urlParam, string key)
        {
            var dic = new Dictionary<string, object>();
            if (urlParam.IndexOf('&') > 0)
            {
                foreach (var temp in urlParam.Split('&'))
                {
                    if (temp.IndexOf('=') > 0 && temp.Split('=')[0].ToLower() == key.ToLower())
                        return temp.Split('=')[1];
                }
            }
            else
            {
                if (urlParam.IndexOf('=') > 0 && urlParam.Split('=')[0].ToLower() == key.ToLower())
                    return urlParam.Split('=')[1];
            }

            return "";
        }
        #endregion

        #region 获取客户Ip
        /// <summary>
        /// 获取客户Ip
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetClientIp(HttpContext context)
        {
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ip))
                ip = context.Connection.RemoteIpAddress.ToString();
            return ip;
        }
        #endregion

        #region 日志
        /// <summary>
        /// 日志
        /// </summary>
        /// <param name="log"></param>
        /// <param name="context"></param>
        /// <param name="stopwatch"></param>
        /// <param name="key"></param>
        /// <param name="db"></param>
        public static void Log(HttpContext context, Stopwatch stopwatch, string key, DataContext db, string urlParam)
        {
            var log = new ApiLog();
            stopwatch.Stop();
            log.Key = key;

            if (stopwatch.Elapsed.TotalMilliseconds.ToStr().Length > 12)
                log.Milliseconds = stopwatch.Elapsed.TotalMilliseconds.ToStr().Substring(0, 12);
            else
                log.Milliseconds = stopwatch.Elapsed.TotalMilliseconds.ToStr();

            log.VisitTime = DateTime.Now;
            log.AppSecret = GetUrlParam(urlParam, "AppSecret");
            log.Ip = GetClientIp(context);
            log.Param = urlParam;

            if (!string.IsNullOrEmpty(key) && key.ToLower() != "favicon.ico")
                db.Add(log);
        }
        #endregion
    }
}
