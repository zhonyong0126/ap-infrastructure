using Ap.Infrastructure;

namespace Ap.Web
{
    /// <summary>
    /// 分布查询请求参数
    /// </summary>
    public class PaginationQueryRequest: IHavePagination
    {
        /// <summary>
        /// 页码
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 页大小
        /// </summary>
        public int PageSize { get; set; }
    }
}
