using FastData.Core;
using FastUntility.Core.Base;
using FastUntility.Core.Page;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fast.Api
{
    public class FastApi : IFastApi
    {
        public async Task ContentAsync(HttpContext context)
        {
            var urlParam = GetUrlParam(context);
            var isSuccess = true;
            var dic = new Dictionary<string, object>();
            var stopwatch = new Stopwatch();

            stopwatch.Start();

            context.Response.ContentType = "application/Json";
            var key = context.Request.Path.Value.ToStr().Replace("/", "").ToUpper();

            if (FastMap.IsExists(key))
            {
                var data = new List<Dictionary<string, object>>();
                var param = new List<DbParameter>();

                foreach (var item in FastMap.MapParam(key))
                {
                    var tempParam = DbProviderFactories.GetFactory(FastMap.MapDb(key)).CreateParameter();
                    tempParam.ParameterName = item;
                    tempParam.Value = GetUrlParam(urlParam, item);

                    if (tempParam.Value.ToStr() != "")
                        param.Add(tempParam);
                }

                var pageSize = GetUrlParam(urlParam, "PageSize");
                var pageId = GetUrlParam(urlParam, "PageId");
                if (pageSize != "" && pageId != "")
                {
                    isSuccess = true;
                    var page = new PageModel();
                    page.PageSize = pageSize.ToInt(0) == 0 ? 10 : pageSize.ToInt(0);
                    page.PageId = pageId.ToInt(0) == 0 ? 1 : pageId.ToInt(0);
                    var info = FastMap.QueryPage(page, key, param.ToArray());
                    dic.Add("data", info.list);
                    dic.Add("page", info.pModel);
                }
                else
                {
                    if (param.Count > 0)
                    {
                        isSuccess = true;
                        data = FastMap.Query(key, param.ToArray());
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
        private static string GetUrlParam(HttpContext context)
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
    }
}
