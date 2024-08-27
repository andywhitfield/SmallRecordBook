namespace SmallRecordBook.Web.Pages.ViewModels;

public record UserNavModel(
    IEnumerable<(string Tag, int TagCount)> Tags,
    int OverdueReminderCount,
    HttpRequest Request)
{
    private const string _selectedCssClass = "srb-selected";

    public string Css(string prop, string propVal = "") =>
        prop switch {
            "all" when Request.Path == new PathString("/") => _selectedCssClass,
            "reminders" when Request.Path.StartsWithSegments(new PathString("/reminders")) => _selectedCssClass + (OverdueReminderCount > 0 ? " srb-list-due" : ""),
            "reminders" => OverdueReminderCount > 0 ? " srb-list-due" : "",
            _ => ""
        };
}