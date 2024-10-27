namespace SmallRecordBook.Web;

public class Pagination
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 500;

    public static readonly Pagination Empty = new(0, 0, null, null, null);

    public static (IEnumerable<T> Items, int Page, int PageCount) Paginate<T>(IEnumerable<T> items, int page, int? pageSize = null, Func<T, bool>? selectedItem = null)
    {
        var actualPageSize = Math.Min(MaxPageSize, Math.Max(1, pageSize ?? DefaultPageSize));

        var itemCount = items.Count();
        if (itemCount <= actualPageSize || page < 1)
            return (items, 1, 1);

        if (selectedItem != null)
        {
            var pageForSelectedItem = items.Chunk(actualPageSize).Select((pgItems, pgIdx) => pgItems.Any(i => selectedItem(i)) ? pgIdx + 1 : 0).FirstOrDefault(pgNum => pgNum > 0);
            if (pageForSelectedItem > 0)
                page = pageForSelectedItem;
        }

        var pageCount = (itemCount / actualPageSize) + (itemCount % actualPageSize == 0 ? 0 : 1);
        var skip = Math.Min(page, pageCount);
        return (items.Skip((skip - 1) * actualPageSize).Take(actualPageSize), skip, pageCount);
    }

    public Pagination(int pageNumber, int pageCount, string? tag, Guid? link, string? find)
    {
        PageNumber = pageNumber;
        PageCount = pageCount;
        Tag = tag;
        Link = link;
        Find = find;

        if (PageCount <= 10)
        {
            Pages = Enumerable.Range(1, PageCount).Select(p => new Page(p, pageNumber == p));
        }
        else
        {
            List<int> pages = new();
            pages.AddRange(Enumerable.Range(pageNumber - 2, 6));
            Normalise(pages);

            if (pageNumber - 4 <= 1)
            {
                pages.InsertRange(0, Enumerable.Range(pages[0] - 2, 2));
                Normalise(pages);
                pages.Add(PageCount);
            }
            else if (pages[pages.Count - 1] + 2 >= PageCount)
            {
                pages.AddRange(Enumerable.Range(pages[pages.Count - 1] + 1, 2));
                Normalise(pages);
                pages.Insert(0, 1);
            }
            else
            {
                pages.Insert(0, 1);
                pages.Add(PageCount);
            }

            Pages = pages.Select((p, idx) => new Page(p, pageNumber == p, idx + 1 < pages.Count && pages[idx + 1] != p + 1));

            void Normalise(List<int> pgs)
            {
                var adjust = pgs[0] < 1
                    ? ((pgs[0] * -1) + 1)
                    : (pgs[pgs.Count - 1] > PageCount
                        ? (PageCount - pgs[pgs.Count - 1])
                        : 0);

                if (adjust != 0)
                    for (var i = 0; i < pgs.Count; i++)
                        pgs[i] += adjust;
            }
        }
    }

    public int PageNumber { get; }
    public int PageCount { get; }
    public IEnumerable<Page> Pages { get; }
    public string? Tag { get; }
    public Guid? Link { get; }
    public string? Find { get; }
}