using System.Collections.Generic;
using System.Linq;
using Ap.Infrastructure;

namespace Ap.Web
{
    /// <summary>
    /// 分页查询返回值
    /// </summary>
    public class PaginationQueryResponse<TData> : IHavePagination where TData:IResponse
    {
        public PaginationQueryResponse(TData[] data)
        {
            Data=data;
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="data"></param>
        public PaginationQueryResponse(IEnumerable<TData> data)
        {
            Data=data.ToArray();
        }
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总大小
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// 数据 
        /// </summary>
        public TData[] Data { get;}
    }
}
