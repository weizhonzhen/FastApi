using FastData.Core.Property;

namespace Fast.Api.DataModel
{
    /// <summary>
    /// 子map
    /// </summary>
    [Table(Comments = "子map")]
    public class ApiMapLeaf
    {
        /// <summary>
        /// map语句id
        /// </summary>
        [Column(Comments = "map语句id", DataType = "varchar2", Length = 32, IsNull = false)]
        public string MapId { get; set; }
        
        /// <summary>
        /// 子map语句id
        /// </summary>
        [Column(Comments = "子map语句id", DataType = "varchar2", Length = 32, IsNull = false)]
        public string LeafMapId { get; set; }

        /// <summary>
        /// map备注
        /// </summary>
        [Column(Comments = "参数备注", DataType = "varchar2", Length = 64)]
        public string Remark { get; set; }
        
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
        
        /// <summary>
        /// 结果参数名(多个|隔开)
        /// </summary>
        [Column(Comments = "结果参数名(多个|隔开)", DataType = "varchar2", Length = 255, IsNull = true)]
        public string ResultParam { get; set; }
    }
}
