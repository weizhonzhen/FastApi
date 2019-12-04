using Microsoft.AspNetCore.Mvc;
using System;
using FastUntility.Core.Base;
using System.Text;
using System.Collections.Generic;

namespace Fast.Api.Mvc.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 加载帮助文档
        /// </summary>
        /// <returns></returns>
        [Route("help")]
        public IActionResult Index()
        {
            var aa = FastData.Core.FastMap.Api;
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
            var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

            foreach(var item in map.Path)
            {
                if (System.IO.File.Exists(item))
                    model.Add(item, System.IO.File.ReadAllText(item));
            }

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
                    return Json(new { msg = "xml或文件名不能为空" });
                else if (string.IsNullOrEmpty(name.ToLower().Replace(".xml", "")))
                    return Json(new { msg = "xml文件名填写不正确" });
                else
                {
                    var xmlPath = string.Format("map/{0}",  name);
                    using (var xmlWrite = System.IO.File.Create(xmlPath))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(xml));
                    }

                    var info = new System.IO.FileInfo(xmlPath);

                    var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

                    if (!map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", name.ToLower())))
                    {
                        var dic = new Dictionary<string, object>();
                        map.Path.Add(string.Format("map/{0}", name));
                        dic.Add("SqlMap", map);
                        var json = BaseJson.ModelToJson(dic);
                        System.IO.File.WriteAllText("map.json", json);
                    }

                    FastData.Core.FastMap.InstanceMap();
                    return Json(new { msg = "操作成功" });
                }
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.StackTrace, "xml");
                return Json(new { msg = ex.Message });
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
                var xmlPath = string.Format("map/{0}", name);
                System.IO.File.Delete(xmlPath);

                var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");
                if (map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", name.ToLower())))
                {
                    var dic = new Dictionary<string, object>();
                    map.Path.Remove("map/" + name);
                    dic.Add("SqlMap", map);
                    var json = BaseJson.ModelToJson(dic);
                    System.IO.File.WriteAllText("map.json", json);

                    FastData.Core.FastMap.InstanceMap();
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
