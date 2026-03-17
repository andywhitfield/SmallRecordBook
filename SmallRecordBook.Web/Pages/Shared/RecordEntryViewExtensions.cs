using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Pages.Shared;

public static class RecordEntryViewExtensions
{
    public static string CcyAmount(this RecordEntry recordEntry)
    {
        if (recordEntry.Currency == null || recordEntry.Amount == null)
            return "";
        return $"{recordEntry.Currency}{FormattedAmount(recordEntry.Amount)}";
    }

    public static string FormattedAmount(this decimal? amount, bool includeThousandSeparator = true)
    {
        if (amount == null)
            return "";
        // poor man's formatting - simply format to at least 2dp if a non-integer value
        // could look at using the C specifier and use the correct culture
        var format = includeThousandSeparator ? "#,##0" : "0";
        if (!decimal.IsInteger(amount.Value))
            format += ".00####";
        return amount.Value.ToString(format);
    }
}
