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
        /// map语句id
        /// </summary>
        [Column(Comments = "map语句id", DataType = "varchar2", Length = 16, IsNull = false)]
        public string MapId { get; set; }

        /// <summary>
        /// 是否写 1=写,0=读取
        /// </summary>
        [Column(Comments = "是否写 1=写,0=读取", DataType = "number(1,0)")]
        public decimal IsWrite { get; set; }

        /// <summary>
        /// 是否匿名访问 1=是,0=否
        /// </summary>
        [Column(Comments = "是否匿名访问 1=是,0=否", DataType = "number(1,0)")]
        public decimal IsAnonymous { get; set; }

        /// <summary>
        /// 数据库key
        /// </summary>
        [Column(Comments = "数据库key", DataType = "varchar2", Length = 16, IsNull = false)]
        public string DbKey { get; set; }
    }
}
