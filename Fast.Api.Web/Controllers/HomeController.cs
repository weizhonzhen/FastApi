using Microsoft.AspNetCore.Mvc;
using System;
using FastUntility.Core.Base;
using System.Text;
using System.Collections.Generic;

namespace Fast.Api.Web.Controllers
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
                var path = string.Format("{0}/{1}", AppDomain.CurrentDomain.BaseDirectory, item);
                if (System.IO.File.Exists(path))
                    model.Add(item, System.IO.File.ReadAllText(path));
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
                    var xmlPath = string.Format("{0}/map/{1}", AppDomain.CurrentDomain.BaseDirectory, name);
                    using (var xmlWrite = System.IO.File.Create(xmlPath))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(xml));
                    }

                    var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

                    if (!map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", name.ToLower())))
                    {
                        var mapPath = string.Format("{0}/map.json", AppDomain.CurrentDomain.BaseDirectory);
                        var dic = new Dictionary<string, object>();
                        map.Path.Add(string.Format("map/{0}", name));
                        dic.Add("SqlMap", map);
                        var json = BaseJson.ModelToJson(dic);
                        System.IO.File.WriteAllText(mapPath, json);
                    }

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
                var xmlPath = string.Format("{0}/map/{1}", AppDomain.CurrentDomain.BaseDirectory, name);
                System.IO.File.Delete(xmlPath);

                var mapPath = string.Format("{0}/map.json", AppDomain.CurrentDomain.BaseDirectory);
                var dic = new Dictionary<string, object>();
                var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");
                map.Path.Remove("map/" + name);
                dic.Add("SqlMap", map);
                var json = BaseJson.ModelToJson(dic);
                System.IO.File.WriteAllText(mapPath, json);

                return Json(new { msg = "操作成功" });
            }
        }
    }
    
    public class SqlMap
    {
        public List<string> Path { get; set; }
    }
}
