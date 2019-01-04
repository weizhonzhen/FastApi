using FastData.Core.Property;

namespace Fast.Api.DataModel
{
    [Table(Comments ="接口")]
    public class ApiData
    {
        /// <summary>
        /// 接口key
        /// </summary>
        [Column(Comments = "接口key", DataType = "varchar2", Length = 16, IsKey = true)]
        public string Key { get; set; }
        
        /// <summary>
        /// 接口名称
        /// </summary>
        [Column(Comments = "接口名称", DataType = "varchar2", Length = 32,IsNull =false)]
        public string Name { get; set; }
        
        /// <summary>
        /// 是否匿名访问 1=是,0=否
        /// </summary>
        [Column(Comments = "是否匿名访问 1=是,0=否", DataType = "number(1,0)")]
        public decimal IsAnonymous { get; set; }
    }
}
