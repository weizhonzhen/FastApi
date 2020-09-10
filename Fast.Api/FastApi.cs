using FastData.Core;
using FastUntility.Core.Base;
using FastUntility.Core.Page;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using FastData.Core.Repository;

namespace Fast.Api
{
    public class FastApi : IFastApi
    {
        public async Task ContentAsync(HttpContext context, IFastRepository IFast)
        {
            var urlParam = HttpUtility.UrlDecode(GetUrlParam(context));
            var isSuccess = true;
            var dic = new Dictionary<string, object>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            context.Response.ContentType = "application/Json";
            var name = context.Request.Path.Value.ToStr().Substring(1, context.Request.Path.Value.ToStr().Length - 1).ToLower();

            if (FastMap.IsExists(name))
            {
                var data = new List<Dictionary<string, object>>();
                var param = new List<DbParameter>();

                foreach (var item in IFast.MapParam(name))
                {                    
                    var checkKey = FastMap.MapCheckMap(name, item);
                    var existsKey = FastMap.MapExistsMap(name, item);
                    var tempParam = DbProviderFactories.GetFactory(IFast.MapDb(name)).CreateParameter();
                    tempParam.ParameterName = item;
                    tempParam.Value = GetUrlParam(urlParam, item);
                    param.Add(tempParam);
                    
                    if (!string.IsNullOrEmpty(FastMap.MapRequired(name, item)))
                    {
                        if (!(FastMap.MapRequired(name, item).ToLower() == "true" && !string.IsNullOrEmpty(tempParam.Value.ToStr())))
                        {
                            dic.Add("error", string.Format("{0}不能为空", item));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (FastMap.MapMaxlength(name, item).ToInt(0) != 0)
                    {
                        if (!(FastMap.MapMaxlength(name, item).ToInt(0) >= tempParam.Value.ToStr().Length))
                        {
                            dic.Add("error", string.Format("{0}：{1}，最大长度{2}", item, tempParam.Value.ToStr(), FastMap.MapMaxlength(name, item)));
                            param.Remove(tempParam);
                            break;
                        }                        
                    }

                    if (!string.IsNullOrEmpty(existsKey))
                    {
                        var existsListParam = new List<DbParameter>();
                        var existsParam = DbProviderFactories.GetFactory(IFast.MapDb(existsKey)).CreateParameter();
                        existsParam.ParameterName = item;
                        existsParam.Value = tempParam.Value;
                        existsListParam.Add(existsParam);

                        var checkData = IFast.Query(existsKey, existsListParam.ToArray())?.First() ?? new Dictionary<string, object>();
                        if (checkData.GetValue("count").ToStr().ToInt(0) >= 1)
                        {
                            dic.Add("error", string.Format("{0}：{1}已存在", item, tempParam.Value));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(checkKey))
                    {
                        var checkListParam = new List<DbParameter>();
                        var checkParam = DbProviderFactories.GetFactory(IFast.MapDb(checkKey)).CreateParameter();
                        checkParam.ParameterName = item;
                        checkParam.Value = GetUrlParam(urlParam, item);
                        checkListParam.Add(checkParam);

                        var checkData = IFast.Query(existsKey, checkListParam.ToArray())?.First() ?? new Dictionary<string, object>();
                        if (checkData.GetValue("count").ToStr().ToInt(0) < 1)
                        {
                            dic.Add("error", string.Format("{0}：{1}不存在", item, tempParam.Value));
                            param.Remove(tempParam);
                            break;
                        }
                    }

                    if (FastMap.MapDate(name, item).ToLower() == "true")
                    {
                        if (!BaseRegular.IsDate(tempParam.Value.ToStr()))
                        {
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

                if (FastMap.MapType(name).ToLower() == AppConfig.PageAll&&dic.Count==0)
                {                    
                    var pageSize = GetUrlParam(urlParam, "PageSize");
                    var pageId = GetUrlParam(urlParam, "PageId");
                    isSuccess = true;
                    var page = new PageModel();

                    page.PageSize = pageSize.ToInt(0) == 0 ? 10 : pageSize.ToInt(0);
                    page.PageId = pageId.ToInt(0) == 0 ? 1 : pageId.ToInt(0);
                    var info = IFast.QueryPage(page, name, param.ToArray());
                    dic.Add("data", info.list);
                    dic.Add("page", info.pModel);
                }
                else if (FastMap.MapType(name).ToLower() == AppConfig.Page && param.Count > 0)
                {
                    var pageSize = GetUrlParam(urlParam, "PageSize");
                    var pageId = GetUrlParam(urlParam, "PageId");
                    isSuccess = true;
                    var page = new PageModel();

                    page.PageSize = pageSize.ToInt(0) == 0 ? 10 : pageSize.ToInt(0);
                    page.PageId = pageId.ToInt(0) == 0 ? 1 : pageId.ToInt(0);
                    var info = IFast.QueryPage(page, name, param.ToArray());
                    dic.Add("data", info.list);
                    dic.Add("page", info.pModel);
                }
                else if (FastMap.MapType(name).ToLower() == AppConfig.All && dic.Count == 0)
                {
                    isSuccess = true;
                    data = IFast.Query(name, param.ToArray());
                    dic.Add("data", data);
                }
                else if (FastMap.MapType(name).ToLower() == AppConfig.Write && param.Count > 0)
                {
                    var result = IFast.Write(name, param.ToArray());
                    if (result.IsSuccess)
                        isSuccess = true;
                    else
                    {
                        isSuccess = false;
                        dic.Add("error", result.Message);
                    }
                }
                else
                { 
                    if (param.Count > 0)
                    {
                        isSuccess = true;
                        data = IFast.Query(name, param.ToArray());
                        dic.Add("data", data);
                    }
                    else
                        isSuccess = false;
                }
            }
            else
            {
                isSuccess = false;
                dic.Add("error", "接口不存在");
            }

            dic.Add("isSuccess", isSuccess);
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(BaseJson.ModelToJson(dic), Encoding.UTF8).ConfigureAwait(false);
        }
        
        #region 获取参数
        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string GetUrlParam(HttpContext context)
        {
            using (var content = new StreamReader(context.Request.Body))
            {
                var param = context.Request.QueryString.Value;

                if (string.IsNullOrEmpty(param))
                    param = content.ReadToEnd();

                if (!string.IsNullOrEmpty(param) && param.Substring(0, 1) == "?")
                    param = param.Substring(1, param.Length - 1);

                return param;
            }
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
