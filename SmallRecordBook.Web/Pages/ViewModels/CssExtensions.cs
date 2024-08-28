using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Pages.ViewModels;

public static class CssExtensions
{
    public static string ListItemCss(this RecordEntry recordEntry)
    {
        List<string> styles = ["srb-list-item"];
        if (recordEntry.ReminderDate != null)
        {
            if (recordEntry.ReminderDone ?? false)
            {
                styles.Add("srb-list-item-done");
            }
            else
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                if (recordEntry.ReminderDate.Value == today)
                    styles.Add("srb-list-item-due");
                if (recordEntry.ReminderDate.Value < today)
                    styles.Add("srb-list-item-overdue");
            }
        }
        return string.Join(' ', styles);
    }
}