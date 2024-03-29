﻿using FastUntility.Core.Base;
using FastUntility.Core.Page;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FastData.Core.Repository;
using FastData.Core.Context;
using FastData.Core.Base;

namespace Fast.Api
{
    public class FastApi : IFastApi
    {
        public async Task ContentAsync(HttpContext context, IFastRepository IFast,  bool IsResource, string dbFile = "db.json")
        {
            var name = context.Request.Path.Value.ToStr().Substring(1, context.Request.Path.Value.ToStr().Length - 1).ToLower();

            var urlParam = HttpUtility.UrlDecode(GetUrlParam(context));
            var success = true;
            var dic = new Dictionary<string, object>();
            var data = new List<Dictionary<string, object>>();
            var dbKey = IFast.MapDb(name).ToStr();
            var pageInfo = new PageResult();

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/Json";

            if (!IFast.IsExists(name))
            {
                dic.Add("success", false);
                dic.Add("error", "接口不存在");
                await context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8).ConfigureAwait(false);
            }
            else if (dbKey == "")
            {
                dic.Add("success", false);
                dic.Add("error", string.Format("map id {0}的db没有配置", name));
                await context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8).ConfigureAwait(false);
            }
            else if (IFast.IsExists(name))
            {
                var param = new List<DbParameter>();

                foreach (var item in IFast.MapParam(name))
                {
                    var checkKey = IFast.MapCheck(name, item);
                    var existsKey = IFast.MapExists(name, item);
                    var tempParam = DbProviderFactories.GetFactory(IFast.MapDb(name), IsResource, dbFile).CreateParameter();

                    tempParam.ParameterName = item;
                    tempParam.Value = GetUrlParam(urlParam, item);
                    param.Add(tempParam);

                    if (!string.IsNullOrEmpty(IFast.MapRequired(name, item)))
                    {
                        if (!(string.Compare(IFast.MapRequired(name, item), "true", true) == 0 && !string.IsNullOrEmpty(tempParam.Value.ToStr())))
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}不能为空", item));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (IFast.MapMaxlength(name, item).ToInt(0) != 0)
                    {
                        if (!(IFast.MapMaxlength(name, item).ToInt(0) >= tempParam.Value.ToStr().Length))
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1}，最大长度{2}", item, tempParam.Value.ToStr(), IFast.MapMaxlength(name, item)));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(existsKey))
                    {
                        var existsListParam = new List<DbParameter>();
                        var existsParam = DbProviderFactories.GetFactory(IFast.MapDb(existsKey), IsResource, dbFile).CreateParameter();
                        existsParam.ParameterName = item;
                        existsParam.Value = tempParam.Value;
                        existsListParam.Add(existsParam);

                        var checkData = IFast.Query(existsKey, existsListParam.ToArray())?.First() ?? new Dictionary<string, object>();
                        if (checkData.GetValue("count").ToStr().ToInt(0) >= 1)
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1}已存在", item, tempParam.Value));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(checkKey))
                    {
                        var checkListParam = new List<DbParameter>();
                        var checkParam = DbProviderFactories.GetFactory(IFast.MapDb(checkKey), IsResource, dbFile).CreateParameter();
                        checkParam.ParameterName = item;
                        checkParam.Value = GetUrlParam(urlParam, item);
                        checkListParam.Add(checkParam);

                        var checkData = IFast.Query(existsKey, checkListParam.ToArray())?.First() ?? new Dictionary<string, object>();
                        if (checkData.GetValue("count").ToStr().ToInt(0) < 1)
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1}不存在", item, tempParam.Value));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (string.Compare(IFast.MapDate(name, item).ToStr(), "true", true) == 0)
                    {
                        if (!BaseRegular.IsDate(tempParam.Value.ToStr()))
                        {
                            dic.Add("success", false);
                            dic.Add("error", string.Format("{0}：{1},不是日期类型", item, tempParam.Value));
                            param.Remove(tempParam);
                            break;
                        }
                        tempParam.Value = tempParam.Value.ToDate();
                        tempParam.DbType = System.Data.DbType.DateTime;
                    }

                    if (tempParam.Value.ToStr() == "")
                        param.Remove(tempParam);
                }
                using (var db = new DataContext(dbKey))
                {
                    var tempParam = param.ToArray();
                    var sql = MapXml.GetMapSql(name, ref tempParam, db, dbKey);

                    if (dic.Count > 0)
                        await context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8).ConfigureAwait(false);
                    else if (string.Compare(IFast.MapType(name).ToStr(), AppConfig.PageAll, true) == 0 ||
                        string.Compare(IFast.MapType(name).ToStr(), AppConfig.Page, true) == 0)
                    {
                        success = true;
                        var pageSize = GetUrlParam(urlParam, "PageSize");
                        var pageId = GetUrlParam(urlParam, "PageId");
                        var page = new PageModel();

                        page.PageSize = pageSize.ToInt(0) == 0 ? 10 : pageSize.ToInt(0);
                        page.PageId = pageId.ToInt(0) == 0 ? 1 : pageId.ToInt(0);
                        pageInfo = db.GetPageSql(page, sql, tempParam).PageResult;
                        if (IFast.MapView(name).ToStr() == "")
                        {
                            dic.Add("data", pageInfo.list);
                            dic.Add("page", pageInfo.pModel);
                        }
                    }
                    else if (string.Compare(IFast.MapType(name).ToStr(), AppConfig.All, true) == 0)
                    {
                        success = true;
                        data = db.ExecuteSqlList(sql, tempParam, false).DicList;
                        dic.Add("data", data);
                    }
                    else if (string.Compare(IFast.MapType(name).ToStr(), AppConfig.Write, true) == 0 && param.Count > 0)
                    {
                        var result = db.ExecuteSqlList(sql, tempParam, false).WriteReturn;
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

                dic.Add("success", success);
                await context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8).ConfigureAwait(false);
            }
        }

        #region 获取参数
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetUrlParam(HttpContext context)
        {
            var param = context.Request.QueryString.Value;
            if (context.Request.ContentLength != null)
            {
                using (var content = new StreamReader(context.Request.Body))
                {
                    if (string.IsNullOrEmpty(param))
                        param = content.ReadToEndAsync().Result;

                    if (!string.IsNullOrEmpty(param) && param.Substring(0, 1) == "?")
                        param = param.Substring(1, param.Length - 1);
                }
            }
            else if (!string.IsNullOrEmpty(param) && param.Substring(0, 1) == "?")
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
        private string GetUrlParam(string urlParam, string key)
        {
            if (urlParam.IndexOf('&') > 0)
            {
                foreach (var temp in urlParam.Split('&'))
                {
                    if (temp.IndexOf('=') > 0 && string.Compare(temp.Split('=')[0], key, true) == 0)
                        return temp.Split('=')[1];
                }
            }
            else
            {
                if (urlParam.IndexOf('=') > 0 && string.Compare(urlParam.Split('=')[0], key, true) == 0)
                    return urlParam.Split('=')[1];
            }

            return "";
        }
        #endregion
    }

    public static class AppConfig
    {
        //读 分页参数必须传
        public static readonly string Page = "page";

        //读 分页参数可以不传
        public static readonly string PageAll = "pageall";

        //读 必须传参数
        public static readonly string Param = "param";

        //读 参数可以不传
        public static readonly string All = "all";

        //写
        public static readonly string Write = "write";

        //接口界面不显示
        public static readonly string Hide = "hide";
    }
}