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

    public void OnGet() => EntryDate ??= DateTime.Today.ToString("yyyy-MM-dd");

    public async Task<IActionResult> OnPost()
    {
        var entryDate = EntryDate.ParseDateOnly();
        var reminderDate = RemindDate.ParseDateOnly(null);
        logger.LogDebug("Creating new record entry on [{EntryDate}][{ParsedEntryDate}] with title [{Title}]; description [{Description}]; reminder date [{RemindDate}][{ParsedRemindDate}]; tags [{Tags}]",
            EntryDate, entryDate, Title, Description, RemindDate, reminderDate, Tags);
        var newRecordEntry = await recordRepository.AddAsync(
            await userAccountRepository.GetUserAccountAsync(User),
            entryDate, Title ?? "", Description, reminderDate, Tags);

        logger.LogInformation("Created new record entry on [{EntryDate}] with title [{Title}]; description [{Description}]; reminder date [{ReminderDate}]; tags [{Tags}]", newRecordEntry.EntryDate, newRecordEntry.Title, newRecordEntry.Description, newRecordEntry.ReminderDate, string.Join(',', newRecordEntry.ActiveRecordEntryTags?.Select(t => t.Tag) ?? []));
        return Redirect("/");
    }
}
