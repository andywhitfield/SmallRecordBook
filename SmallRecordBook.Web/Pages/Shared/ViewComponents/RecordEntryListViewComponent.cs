using Microsoft.AspNetCore.Mvc;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Pages.ViewModels;

namespace SmallRecordBook.Web.Pages.Shared.ViewComponents;

public class RecordEntryListViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<RecordEntry> recordEntries)
        => View(new RecordEntryListModel(recordEntries));
}