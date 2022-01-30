namespace Deployf.Botf;

public class Paging<TItem>
{
    public int Count { get; set; }
    public int ItemsPerPage { get; set; }
    public int PageNumber { get; set; }
    public IEnumerable<TItem> Items { get; set; }

    public Paging(int count, int itemsPerPage, int page, IEnumerable<TItem> items)
    {
        Count = count;
        ItemsPerPage = itemsPerPage;
        PageNumber = page;
        Items = items;
    }
}