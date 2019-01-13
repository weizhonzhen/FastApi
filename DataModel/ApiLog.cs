using FastData.Core.Property;
using System;

namespace Fast.Api.DataModel
{
    [Table(Comments = "接口日志")]
    public class ApiLog
    {
        /// <summary>
        /// 接口
        /// </summary>
        [Column(Comments = "接口", DataType = "varchar2", Length = 16)]
        public string Key { get; set; }

        /// <summary>
        /// 密钥
        /// </summary>
        [Column(Comments = "密钥", DataType = "varchar2", Length = 32,IsNull =true)]
        public string AppSecret { get; set; }

        /// <summary>
        /// 访问时间
        /// </summary>
        [Column(Comments = "访问时间", DataType = "date")]
        public DateTime VisitTime { get; set; }

        /// <summary>
        /// 来源Ip
        /// </summary>
        [Column(Comments = "来源Ip", DataType = "varchar2", Length = 16)]
        public string Ip { get; set; }
        
        /// <summary>
        /// 参数
        /// </summary>
        [Column(Comments = "参数", DataType = "varchar2", Length = 255,IsNull =true)]
        public string Param { get; set; }

        /// <summary>
        /// 请求时间秒
        /// </summary>
        [Column(Comments = "请求时间秒", DataType = "varchar2", Length = 12)]
        public string Milliseconds { get; set; }
    }
}
