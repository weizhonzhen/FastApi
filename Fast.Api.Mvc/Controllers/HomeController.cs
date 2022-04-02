using Microsoft.AspNetCore.Mvc;
using System;
using FastUntility.Core.Base;
using System.Text;
using System.Collections.Generic;
using FastData.Core;
using FastData.Core.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Fast.Api.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFastRepository IFast;

        public HomeController(IFastRepository _IFast)
        {
            IFast = _IFast;
        }

        /// <summary>
        /// 加载帮助文档
        /// </summary>
        /// <returns></returns>
        [Route("help")]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 加载xml
        /// </summary>
        /// <returns></returns>
        [Route("xml")]
        public IActionResult Xml()
        {
            var model = new Dictionary<string, object>();
            var map = BaseConfig.GetValue<SqlMap>("SqlMap", FastApiExtension.config.mapFile);

            map.Path.ForEach(a => {
                if (System.IO.File.Exists(a))
                    model.Add(a, System.IO.File.ReadAllText(a));
            });

            ViewData.Model = model;
            return View();
        }

        /// <summary>
        /// 编辑xml
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("xml")]
        public IActionResult Xml(string xml, string name)
        {
            try
            {
                if (string.IsNullOrEmpty(xml) || string.IsNullOrEmpty(name))
                    return Json(new { msg = "xml或文件名不能为空", Issuccess = false });
                else if (string.IsNullOrEmpty(name.ToLower().Replace(".xml", "")))
                    return Json(new { msg = "xml文件名填写不正确", Issuccess = false });
                else
                {
                    var xmlPath = string.Format("map/{0}",  name);
                    using (var xmlWrite = System.IO.File.Create(xmlPath))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(xml));
                    }

                    if (IFast.CheckMap(xmlPath))
                    {
                        var map = BaseConfig.GetValue<SqlMap>("SqlMap", FastApiExtension.config.mapFile) ;

                        if (!map.Path.Exists(a => string.Compare(a, name) == 0))
                        {
                            var dic = new Dictionary<string, object>();
                            map.Path.Add(string.Format("map/{0}", name));
                            dic.Add("SqlMap", map);
                            var json = BaseJson.ModelToJson(dic);
                            System.IO.File.WriteAllText(FastApiExtension.config.mapFile, json);
                        }

                        FastMap.InstanceMap();
                        return Json(new { msg = "操作成功", Issuccess = true });
                    }
                    else
                        return Json(new { msg = "操作失败", Issuccess = false });
                }
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.StackTrace, "xml");
                return Json(new { msg = ex.Message, Issuccess = false });
            }
        }

        /// <summary>
        /// 删除xml
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("del")]
        public IActionResult Del(string name)
        {
            if (string.IsNullOrEmpty(name))
                return Json(new { msg = "xml文件名不能为空" });
            else if (string.IsNullOrEmpty(name.ToLower().Replace(".xml", "")))
                return Json(new { msg = "xml文件名填写不正确" });
            else
            {
                System.IO.File.Delete(name);

                var map = BaseConfig.GetValue<SqlMap>("SqlMap", FastApiExtension.config.mapFile);
                if (!map.Path.Exists(a => string.Compare(a, name) == 0))
                {
                    var dic = new Dictionary<string, object>();
                    map.Path.Remove( name);
                    dic.Add("SqlMap", map);
                    var json = BaseJson.ModelToJson(dic);
                    System.IO.File.WriteAllText(FastApiExtension.config.mapFile, json);

                    FastMap.InstanceMap();
                }               

                return Json(new { msg = "操作成功" });
            }
        }
    }
    
    public class SqlMap
    {
        public List<string> Path { get; set; }
    }
}
