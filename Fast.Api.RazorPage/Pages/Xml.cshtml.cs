using FastUntility.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fast.Api.RazorPage.Pages
{
    public class XmlModel : PageModel
    {
        public Dictionary<string,object> Map = new Dictionary<string, object>();
        public void OnGet()
        {
            var xml = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

            foreach (var item in xml.Path)
            {
                var path = string.Format("{0}/{1}", AppDomain.CurrentDomain.BaseDirectory, item);
                if (System.IO.File.Exists(path))
                    Map.Add(item, System.IO.File.ReadAllText(path));
            }
        }

        public IActionResult OnPostXml(SaveParam item)
        {
            var result = new Dictionary<string, object>();
            try
            {
                if (string.IsNullOrEmpty(item.name.ToLower().Replace(".xml", "")))
                    result.Add("msg", "xml文件名填写不正确");
                else
                {
                    var xmlPath = string.Format("{0}/map/{1}", AppDomain.CurrentDomain.BaseDirectory, item.name);
                    using (var xmlWrite = System.IO.File.Create(xmlPath))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(item.xml));
                    }

                    var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

                    if (!map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", item.name.ToLower())))
                    {
                        var mapPath = string.Format("{0}/map.json", AppDomain.CurrentDomain.BaseDirectory);
                        var dic = new Dictionary<string, object>();
                        map.Path.Add(string.Format("map/{0}", item.name));
                        dic.Add("SqlMap", map);
                        var json = BaseJson.ModelToJson(dic);
                        System.IO.File.WriteAllText(mapPath, json);
                    }

                    FastData.Core.FastMap.InstanceMap();
                    result.Add("msg", "操作成功");
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.StackTrace, "xml");
                result.Add("msg", ex.Message);
                return new JsonResult(result);
            }
        }

        public IActionResult OnPostDel(DelParam item)
        {
            var result = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(item.name.ToLower().Replace(".xml", "")))
                result.Add("msg", "xml文件名填写不正确");
            else
            {
                var xmlPath = string.Format("{0}/map/{1}", AppDomain.CurrentDomain.BaseDirectory, item.name);
                System.IO.File.Delete(xmlPath);

                var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");
                if (map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", item.name.ToLower())))
                {
                    var mapPath = string.Format("{0}/map.json", AppDomain.CurrentDomain.BaseDirectory);
                    var dic = new Dictionary<string, object>();
                    map.Path.Remove("map/" + item.name);
                    dic.Add("SqlMap", map);
                    var json = BaseJson.ModelToJson(dic);
                    System.IO.File.WriteAllText(mapPath, json);

                    FastData.Core.FastMap.InstanceMap();
                }

                result.Add("msg", "操作成功");
            }
            return new JsonResult(result);
        }

        private class SqlMap
        {
            public List<string> Path { get; set; }
        }

        public class SaveParam
        {
            [Required(ErrorMessage = "{0}不能为空")]
            [Display(Name = "xml")]
            public string xml { get; set; }

            [Required(ErrorMessage = "{0}不能为空")]
            [Display(Name = "name")]
            public string name { get; set; }
        }

        public class DelParam
        {
            [Required(ErrorMessage = "{0}不能为空")]
            [Display(Name = "name")]
            public string name { get; set; }
        }
    }
}