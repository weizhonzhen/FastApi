using FastData.Core;
using FastData.Core.Repository;
using FastUntility.Core.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fast.Api.RazorPage.Pages
{
    public class XmlModel : PageModel
    {
        public Dictionary<string,object> Map = new Dictionary<string, object>();
        private readonly IFastRepository IFast;

        public XmlModel(IFastRepository _IFast)
        {
            IFast = _IFast;
        }

        public void OnGet()      
        {
            var xml = BaseConfig.GetValue<SqlMap>("SqlMap", FastApiExtension.config.mapFile, false);

            xml.Path.ForEach(a => {
                if (System.IO.File.Exists(a))
                    Map.Add(a, System.IO.File.ReadAllText(a));
            });
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
                    using (var xmlWrite = System.IO.File.Create(item.name))
                    {
                        xmlWrite.Write(Encoding.Default.GetBytes(item.xml));
                    }

                    if (IFast.CheckMap(item.name))
                    {
                        var map = BaseConfig.GetValue<SqlMap>("SqlMap", FastApiExtension.config.mapFile, false);

                        if (!map.Path.Exists(a => string.Compare(a, item.name) == 0))
                        {
                            var dic = new Dictionary<string, object>();
                            map.Path.Add(item.name);
                            dic.Add("SqlMap", map);
                            var json = BaseJson.ModelToJson(dic);
                            System.IO.File.WriteAllText(FastApiExtension.config.mapFile, json);
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
                System.IO.File.Delete(item.name);

                var map = BaseConfig.GetValue<SqlMap>("SqlMap", FastApiExtension.config.mapFile, false);
                if (!map.Path.Exists(a => string.Compare(a, item.name) == 0))
                {
                    var dic = new Dictionary<string, object>();
                    map.Path.Remove(item.name);
                    dic.Add("SqlMap", map);
                    var json = BaseJson.ModelToJson(dic);
                    System.IO.File.WriteAllText(FastApiExtension.config.mapFile ,json);

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