using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmallRecordBook.Web.Pages.ViewModels;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages;

public class AddModel(ILogger<AddModel> logger,
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public string? EntryDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? Title { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? Description { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? RemindDate { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? Tags { get; set; } = "";

    [BindProperty(SupportsGet = true)] public int? Parent { get; set; }
    [BindProperty] public string? Cancel { get; set; }

    public async Task OnGet()
    {
        EntryDate ??= DateTime.Today.ToString("yyyy-MM-dd");

        if (Parent != null)
        {
            logger.LogInformation("Getting parent record entry {Parent}", Parent);
            var userAccount = await userAccountRepository.GetUserAccountAsync(User);
            var recordEntry = await recordRepository.GetByIdAsync(userAccount, Parent.Value);
            if (recordEntry != null)
            {
                Title = recordEntry.Title;
                Description = recordEntry.Description;
                Tags = recordEntry.TagString();
                if (recordEntry.ReminderDate != null)
                    RemindDate = EntryDate.ParseDateOnly().AddDays(Math.Abs(recordEntry.ReminderDate.Value.DayNumber - recordEntry.EntryDate.DayNumber)).ToString("yyyy-MM-dd");
            }
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (Cancel == "cancel")
        {
            logger.LogInformation("Add from existing record {Parent} cancelled, redirecting back to parent", Parent);
            return Redirect(Parent != null ? $"/record/{Parent}" : "/");
        }

        var entryDate = EntryDate.ParseDateOnly();
        var reminderDate = RemindDate.ParseDateOnly(null);
        logger.LogDebug("Creating new record entry on [{EntryDate}][{ParsedEntryDate}] with title [{Title}]; description [{Description}]; reminder date [{RemindDate}][{ParsedRemindDate}]; tags [{Tags}]; parent [{Parent}]",
            EntryDate, entryDate, Title, Description, RemindDate, reminderDate, Tags, Parent);
        var newRecordEntry = await recordRepository.AddAsync(
            await userAccountRepository.GetUserAccountAsync(User),
            entryDate, Title ?? "", Description, reminderDate, Tags, Parent);

        logger.LogInformation("Created new record entry on [{EntryDate}] with title [{Title}]; description [{Description}]; reminder date [{ReminderDate}]; tags [{Tags}]; parent [{Parent}]",
            newRecordEntry.EntryDate, newRecordEntry.Title, newRecordEntry.Description, newRecordEntry.ReminderDate, newRecordEntry.TagString(), Parent);
        return Redirect("/");
    }
}
