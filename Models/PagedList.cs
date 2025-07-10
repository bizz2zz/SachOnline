namespace LuongVinhKhang.SachOnline.Models
{
    public class PagedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PagedList(IEnumerable<T> items, int page, int pageSize, int totalPages)
        {
            PageIndex = page;
            TotalPages = totalPages;
            AddRange(items);
        }
    }
}