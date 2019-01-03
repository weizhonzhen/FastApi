using FastData.Core.Property;

namespace Fast.Api.DataModel
{
    [Table(Comments = "接口参数")]
    public class ApiParam
    {
        /// <summary>
        /// 接口key
        /// </summary>
        [Column(Comments = "接口key", DataType = "varchar2", Length = 16)]
        public string Key { get; set; }

        /// <summary>
        /// 参数名称对应map名称
        /// </summary>
        [Column(Comments = "参数名称对应map名称", DataType = "varchar2", Length = 32, IsNull = false)]
        public string Name { get; set; }

        /// <summary>
        /// 参数备注
        /// </summary>
        [Column(Comments = "参数备注", DataType = "varchar2", Length = 64)]
        public string Remark { get; set; }

        /// <summary>
        /// 参数排序
        /// </summary>
        [Column(Comments = "参数名称对应map名称", DataType = "number(1,0)", IsNull = false)]
        public decimal OderBy { get; set; }
    }
}
