using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Pages.ViewModels;

public static class TagExtensions
{
    public static string TagString(this RecordEntry recordEntry)
        => string.Join(' ', recordEntry.ActiveRecordEntryTags.Select(ret => ret.Tag));
}