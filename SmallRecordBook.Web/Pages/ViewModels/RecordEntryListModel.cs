using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Pages.ViewModels;

public record RecordEntryListModel(IEnumerable<RecordEntry> RecordEntries) { }
