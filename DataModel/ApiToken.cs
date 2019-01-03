using FastData.Core.Property;

namespace Fast.Api.DataModel
{
    [Table(Comments = "接口认证")]
    public class ApiToken
    {
        /// <summary>
        /// 密钥
        /// </summary>
        [Column(Comments = "密钥", DataType = "varchar2", Length = 32, IsKey = true)]
        public string AppSecret { get; set; }

        /// <summary>
        /// 接口Key权限多个逗号隔开
        /// </summary>
        [Column(Comments = "接口Key权限多个逗号隔开", DataType = "varchar2", Length = 255, IsNull = false)]
        public string Key { get; set; }
                
        /// <summary>
        /// 是否绑定Ip 1=是,0=否
        /// </summary>
        [Column(Comments = "是否绑定Ip 1=是,0=否", DataType = "number(1,0)")]
        public decimal IsBindIp { get; set; }
    }
}
