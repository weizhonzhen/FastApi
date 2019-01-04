using FastData.Core.Property;

namespace Fast.Api.DataModel
{
    [Table(Comments = "map参数")]
    public class ApiMap
    {
        /// <summary>
        /// 接口key
        /// </summary>
        [Column(Comments = "接口key", DataType = "varchar2", Length = 16, IsNull = false)]
        public string Key { get; set; }

        /// <summary>
        /// map语句id
        /// </summary>
        [Column(Comments = "map语句id", DataType = "varchar2", Length = 32, IsNull = false)]
        public string MapId { get; set; }

        /// <summary>
        /// map备注
        /// </summary>
        [Column(Comments = "参数备注", DataType = "varchar2", Length = 64)]
        public string Remark { get; set; }

        /// <summary>
        /// 是否写 1=写,0=读取
        /// </summary>
        [Column(Comments = "是否写 1=写,0=读取", DataType = "number(1,0)")]
        public decimal IsWrite { get; set; }

        /// <summary>
        /// 数据库key
        /// </summary>
        [Column(Comments = "数据库key", DataType = "varchar2", Length = 16, IsNull = false)]
        public string DbKey { get; set; }

        /// <summary>
        /// map排序
        /// </summary>
        [Column(Comments = "map排序", DataType = "number(1,0)", IsNull = false)]
        public decimal OderBy { get; set; }
    }
}
