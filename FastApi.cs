using System.Text;
using FastData.Core;
using FastData.Core.Context;
using System.Collections.Generic;
using FastUntility.Core.Base;
using Fast.Api.DataModel;
using System.Data.Common;
using System.Diagnostics;
using System;
using System.Linq;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Fast.Api
{
    public class FastApi : IFastApi
    {
        //接口数据库
        public static string DbApi = "Api";

        public void Content(HttpContext context)
        {
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
                        if (!CheckToken(context, info, db))
                        {
                            isSuccess = false;
                            dic.Add("error", "接口无权访问");
                        }
                    }

                    if (isSuccess)
                    {
                        var paramList = FastRead.Query<ApiParam>(a => a.Key.ToUpper() == key).OrderBy<ApiParam>(a => new { a.OderBy }).ToList<ApiParam>(db);
                        var param = new List<DbParameter>();

                        foreach (var item in paramList)
                        {
                            var tempParam = DbProviderFactories.GetFactory(DbApi).CreateParameter();
                            tempParam.ParameterName = item.Name;
                            tempParam.Value = GetParam(context, item.Name);
                            param.Add(tempParam);
                        }

                        if (info.IsWrite == 1)
                            isSuccess = FastMap.Write(info.MapId, param.ToArray(), null, info.DbKey).IsSuccess;
                        else
                        {
                            isSuccess = true;
                            var data = FastMap.Query(info.MapId, param.ToArray(), null, info.DbKey);
                            dic.Add("data", data);
                        }
                    }
                }
                else
                {
                    isSuccess = false;
                    dic.Add("error", "接口不存在");
                }
                
                Log(context, stopwatch, key, db);
                dic.Add("isSuccess", isSuccess);
                context.Response.StatusCode = 200;
                context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8);
            }
        }

        #region 验证权限访问
        /// <summary>
        /// 验证权限访问
        /// </summary>
        /// <param name="context"></param>
        /// <param name="api"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static bool CheckToken(HttpContext context, ApiData api,DataContext db)
        {
            var AppSecret = GetParam(context, "AppSecret").ToUpper();

            if (FastRead.Query<ApiToken>(a => a.AppSecret.ToUpper() == AppSecret).ToCount(db) > 0)
            {
                var info = FastRead.Query<ApiToken>(a => a.AppSecret.ToUpper() == AppSecret).ToItem<ApiToken>(db);

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

        #region 获取参数值
        /// <summary>
        /// 获取参数值
        /// </summary>
        /// <param name="context"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetParam(HttpContext context,string name)
        { 
            if (context.Request.Method.ToLower() == "get")
              return context.Request.Query[name].ToStr();

            if (context.Request.Method.ToLower() == "post")
                return context.Request.Form[name].ToStr();

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
        public static void Log(HttpContext context, Stopwatch stopwatch,string key,DataContext db)
        { 
            var log = new ApiLog();
            stopwatch.Stop();
            log.Key = key;
            log.Milliseconds = stopwatch.Elapsed.TotalMilliseconds.ToStr();
            log.VisitTime = DateTime.Now;
            log.AppSecret = GetParam(context, "AppSecret");
            log.Ip = GetClientIp(context);

            if (string.IsNullOrEmpty(context.Request.QueryString.Value))
                log.Param = new StreamReader(context.Request.Body).ReadToEnd();
            else
                log.Param = context.Request.QueryString.Value;

            if (!string.IsNullOrEmpty(key) && key.ToLower() != "favicon.ico")
                db.Add(log);
        }
        #endregion
    }
}
