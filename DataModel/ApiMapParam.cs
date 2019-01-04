using FastData.Core.Property;

namespace Fast.Api.DataModel
{
    [Table(Comments = "map参数")]
    public class ApiMapParam
    {
        /// <summary>
        /// map语句id
        /// </summary>
        [Column(Comments = "map语句id", DataType = "varchar2", Length = 32, IsNull = false)]
        public string MapId { get; set; }
        
        /// <summary>
        /// 参数名称
        /// </summary>
        [Column(Comments = "参数名称", DataType = "varchar2", Length = 32, IsNull = false)]
        public string ParamName { get; set; }

        /// <summary>
        /// 参数来源 1=url,2=map
        /// </summary>
        [Column(Comments = "参数来源", DataType = "number(1,0)", IsNull = false)]
        public decimal Source { get; set; }

        /// <summary>
        /// 参数备注
        /// </summary>
        [Column(Comments = "参数备注", DataType = "varchar2", Length = 64)]
        public string Remark { get; set; }
        
        /// <summary>
        /// 参数排序
        /// </summary>
        [Column(Comments = "参数排序", DataType = "number(1,0)", IsNull = false)]
        public decimal OderBy { get; set; }
    }
}
