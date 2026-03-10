using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Pages.ViewModels;

public record RecordEntryListModel(IEnumerable<RecordEntry> RecordEntries, string NoEntriesMessage, Pagination Pagination, string UrlPath)
{
    public string CcyAmount(RecordEntry recordEntry)
    {
        if (recordEntry.Currency == null || recordEntry.Amount == null)
            return "";
        // poor man's formatting - simply format to at least 2dp if a non-integer value
        // could look at using the C specifier and use the correct culture
        var format = decimal.IsInteger(recordEntry.Amount.Value) ? "0" : "0.00####";
        return $"{recordEntry.Currency}{recordEntry.Amount.Value.ToString(format)}";
    }
}
