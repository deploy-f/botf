namespace Deployf.Botf;


public class PagingService
{
    public int PageDefaultCount = 15;

    public PagingService()
    {
    }

    public PagingService(int pageDefaultCount)
    {
        PageDefaultCount = pageDefaultCount;
    }

    public Paging<TResult> Paging<TResult>(IQueryable<TResult> collection, PageFilter pageParams)
    {
        var count = collection.Count();
        var skiping = (pageParams.Count ?? PageDefaultCount) * (pageParams.Page ?? 0);
        var taking = pageParams.Count ?? PageDefaultCount;
        var resultCollection = collection.Skip(skiping).Take(taking);
        return new Paging<TResult>(count, pageParams.Count ?? PageDefaultCount, pageParams.Page ?? 0, resultCollection);
    }
}


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

public class PageFilter
{
    public int? Page { get; set; }
    public int? Count { get; set; }
}
