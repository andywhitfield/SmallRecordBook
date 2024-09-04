namespace SmallRecordBook.Web.Pages.ViewModels;

public record UserNavModel(
    IEnumerable<(string Tag, int TagCount)> Tags,
    int OverdueReminderCount,
    HttpRequest Request)
{
    private const string _selectedCssClass = "srb-selected";

    public string Css(string prop, string propVal = "") =>
        prop switch {
            "all" when Request.Path == new PathString("/") && Request.Query.Count == 0 => _selectedCssClass,
            "reminders" when Request.Path.StartsWithSegments(new PathString("/reminders")) => _selectedCssClass + (OverdueReminderCount > 0 ? " srb-list-due" : ""),
            "reminders" => OverdueReminderCount > 0 ? " srb-list-due" : "",
            "tag" when Request.Path == new PathString("/") && Request.Query.TryGetValue("tag", out var queryTag) && queryTag == propVal => _selectedCssClass,
            "alltags" when Request.Path == new PathString("/tags") => _selectedCssClass,
            _ => ""
        };
}