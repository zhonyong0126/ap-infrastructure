namespace Ap.Infrastructure
{
    public interface IHavePagination
    {
        int PageIndex { get; set; }
        int PageSize { get; set; }
    }
}
