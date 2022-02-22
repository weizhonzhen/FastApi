using System;
using System.Web;
using FastData;
using FastData.Context;
using System.Data.Common;
using System.Collections.Generic;
using FastUntility.Base;
using System.Linq;
using FastData.Base;
using FastUntility.Page;

namespace Fast.Api.Framework
{
    public class FastApiModule : IHttpModule
    {
        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(BeginRequest);
        }

        void BeginRequest(object sender, EventArgs e)
        {
            var success = true;
            var dic = new Dictionary<string, object>();
            var param = new List<DbParameter>();
            var context = ((HttpApplication)sender);
            var key = context.Request.CurrentExecutionFilePath;
            key = string.IsNullOrEmpty(key) ? "" : key.Substring(1, key.Length - 1);
            if (FastMap.IsExists(key))
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/Json";

                var data = new List<Dictionary<string, object>>();
                var pageInfo = new PageResult();
                var dbKey = FastMap.MapDb(key).ToStr();
                var type = FastMap.MapType(key).ToStr();
                var config = FastMap.DbConfig(dbKey);
                var url = context.Request.Form;
                if (url.Count == 0)
                    url = context.Request.QueryString;

                var pageSize = url.GetValues("pageSize").ToStr().ToInt(10);
                var pageId = url.GetValues("pageId").ToStr().ToInt(1);

                foreach (var item in url)
                {
                    var temp = DbProviderFactories.GetFactory(config.ProviderName).CreateParameter();
                    temp.ParameterName = item.ToStr();
                    temp.Value = url.Get(item.ToStr());
                    param.Add(temp);
                    var existsKey = FastMap.MapExists(key, temp.ParameterName);
                    var checkKey = FastMap.MapCheck(key, temp.ParameterName);

                    //required
                    if (!string.IsNullOrEmpty(FastMap.MapRequired(key, temp.ParameterName)))
                    {
                        dic.Add("success", false);
                        dic.Add("error", string.Format("{0}不能为空", item));
                        param.Remove(temp);
                        break;
                    }

                    //max length
                    if (FastMap.MapMaxlength(key, temp.ParameterName).ToInt(0) != 0)
                    {
                        if (!(FastMap.MapMaxlength(key, temp.ParameterName).ToInt(0) >= temp.Value.ToStr().Length))
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1}，最大长度{2}", item, temp.Value.ToStr(), FastMap.MapMaxlength(key, temp.ParameterName)));
                            param.Remove(temp);
                            break;
                        }
                    }

                    //exists
                    if (!string.IsNullOrEmpty(existsKey))
                    {
                        var existsListParam = new List<DbParameter>();
                        var existsParam = DbProviderFactories.GetFactory(config.ProviderName).CreateParameter();
                        existsParam.ParameterName = temp.ParameterName;
                        existsParam.Value = temp.Value;
                        existsListParam.Add(existsParam);

                        var checkData = FastMap.Query(existsKey, existsListParam.ToArray())?.First() ?? new Dictionary<string, object>();
                        if (checkData.GetValue("count").ToStr().ToInt(0) >= 1)
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1}已存在", item, temp.Value));
                            param.Remove(temp);
                            break;
                        }
                    }

                    //check
                    if (!string.IsNullOrEmpty(checkKey))
                    {
                        var checkListParam = new List<DbParameter>();
                        var checkParam = DbProviderFactories.GetFactory(config.ProviderName).CreateParameter();
                        checkParam.ParameterName = temp.ParameterName;
                        checkParam.Value = temp.Value;
                        checkListParam.Add(checkParam);

                        var checkData = FastMap.Query(existsKey, checkListParam.ToArray())?.First() ?? new Dictionary<string, object>();
                        if (checkData.GetValue("count").ToStr().ToInt(0) < 1)
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1}不存在", item, temp.Value));
                            param.Remove(temp);
                            break;
                        }
                    }

                    //date
                    if (string.Compare( FastMap.MapDate(key, temp.ParameterName).ToStr(), "true", true) ==0)
                    {
                        if (!BaseRegular.IsDate(temp.Value.ToStr()))
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1},不是日期类型", item, temp.Value));
                            param.Remove(temp);
                            break;
                        }
                        temp.Value = temp.Value.ToDate();
                        temp.DbType = System.Data.DbType.DateTime;
                    }
                }


                if (dic.Count > 0)
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/Json";
                    context.Response.Write(BaseJson.ModelToJson(dic));
                    context.Response.End();
                }
                else
                {
                    using (var db = new DataContext(dbKey))
                    {
                        var tempParam = param.ToArray();
                        var sql = MapXml.GetMapSql(key, ref tempParam, db, dbKey);

                        if (string.Compare( type , AppConfig.PageAll, true) ==0 || string.Compare( type , AppConfig.Page, true) ==0)
                        {
                            success = true;
                            var page = new PageModel();

                            page.PageSize = pageSize == 0 ? 10 : pageSize;
                            page.PageId = pageId == 0 ? 1 : pageId;
                            pageInfo = db.GetPageSql(page, sql, tempParam).PageResult;
                            dic.Add("data", pageInfo.list);
                            dic.Add("page", pageInfo.pModel);

                        }
                        else if (string.Compare( type , AppConfig.All, true) ==0)
                        {
                            success = true;
                            data = db.ExecuteSqlList(sql, tempParam, false).DicList;
                            dic.Add("data", data);
                        }
                        else if (string.Compare( type , AppConfig.Write, true) ==0 && param.Count > 0)
                        {
                            var result = db.ExecuteSqlList(sql, tempParam, false).writeReturn;
                            if (result.IsSuccess)
                                success = true;
                            else
                            {
                                success = false;
                                dic.Add("error", result.Message);
                            }
                        }
                        else
                        {
                            if (param.Count > 0)
                            {
                                success = true;
                                data = db.ExecuteSqlList(sql, tempParam, false).DicList;
                                dic.Add("data", data);
                            }
                            else
                                success = false;
                        }
                    }
                }

                //if (FastMap.MapView(key).ToStr() != "")
                //{
                //    context.Response.ContentType = "text/html;charset=utf-8";
                //}
                //else
                {
                    dic.Add("success", success);
                    context.Response.Write(BaseJson.ModelToJson(dic));
                    context.Response.End();
                }
            }
        }
    }

    public static class AppConfig
    {
        public static readonly string Page = "page";

        public static readonly string PageAll = "pageall";

        public static readonly string Param = "param";

        public static readonly string All = "all";

        public static readonly string Write = "write";
    }
}
