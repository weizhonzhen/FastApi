using FastData.Core;
using FastData.Core.Repository;
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
        private IFastRepository IFast;

        public XmlModel(IFastRepository _IFast)
        {
            IFast = _IFast;
        }

        public void OnGet()
        {
            var xml = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

            foreach (var item in xml.Path)
            {
                if (System.IO.File.Exists(item))
                    Map.Add(item, System.IO.File.ReadAllText(item));
            }
        }

        public IActionResult OnPostXml(SaveParam item)
        {
            var result = new Dictionary<string, object>();
            try
            {
                if (string.IsNullOrEmpty(item.name.ToLower().Replace(".xml", "")))
                {
                    result.Add("msg", "xml文件名填写不正确");
                    result.Add("Issuccess", false);
                }
                else
                {
                    var xmlPath = string.Format("map/{0}", item.name);
                    using (var xmlWrite = System.IO.File.Create(xmlPath))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(item.xml));
                    }

                    if (IFast.CheckMap(xmlPath))
                    {
                        var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");

                        if (!map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", item.name.ToLower())))
                        {
                            var dic = new Dictionary<string, object>();
                            map.Path.Add(string.Format("map/{0}", item.name));
                            dic.Add("SqlMap", map);
                            var json = BaseJson.ModelToJson(dic);
                            System.IO.File.WriteAllText("map.json", json);
                        }

                       FastMap.InstanceMap();
                        result.Add("msg", "操作成功");
                        result.Add("Issuccess", true);
                    }
                    else
                    {
                        result.Add("msg", "操作失败");
                        result.Add("Issuccess", false);
                    }
                }
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                BaseLog.SaveLog(ex.StackTrace, "xml");
                result.Add("msg", ex.Message);
                result.Add("Issuccess", false);
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
                var xmlPath = string.Format("map/{0}", item.name);
                System.IO.File.Delete(xmlPath);

                var map = BaseConfig.GetValue<SqlMap>("SqlMap", "map.json");
                if (map.Path.Exists(a => a.ToLower() == string.Format("map/{0}", item.name.ToLower())))
                {
                    var dic = new Dictionary<string, object>();
                    map.Path.Remove("map/" + item.name);
                    dic.Add("SqlMap", map);
                    var json = BaseJson.ModelToJson(dic);
                    System.IO.File.WriteAllText("map.json", json);

                   FastMap.InstanceMap();
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