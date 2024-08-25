namespace SmallRecordBook.Web.Pages.ViewModels;

public static class DateTimeExtensions
{
    public static DateOnly ParseDateOnly(this string? date) =>
        DateOnly.TryParseExact(date, "yyyy-MM-dd", out var d) ? d : DateOnly.FromDateTime(DateTime.UtcNow);

    public static string ToDisplayString(this DateOnly? date) =>
        date == null ? "" : date.Value.ToDisplayString();

    public static string ToDisplayString(this DateOnly date) =>
        date.ToString("yyyy-MM-dd");
}