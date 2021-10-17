namespace Deployf.BL.Objects
{
    public class PageDto<TItem>
    {
        public int Count { get; set; }
        public int ItemsPerPage { get; set; }
        public int Page { get; set; }
        public IEnumerable<TItem> Items { get; set; }

        public PageDto(int count, int itemsPerPage, int page, IEnumerable<TItem> items)
        {
            Count = count;
            ItemsPerPage = itemsPerPage;
            Page = page;
            Items = items;
        }
    }
}
