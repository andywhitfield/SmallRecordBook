using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmallRecordBook.Web.Pages.ViewModels;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages;

public class RecordModel(ILogger<RecordModel> logger,
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository)
    : PageModel
{
    public int EntryId { get; set; }
    [BindProperty(SupportsGet = true)] public string? EntryDate { get; set; }
    [BindProperty(SupportsGet = true)] public string? Title { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? Description { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? RemindDate { get; set; } = "";
    [BindProperty(SupportsGet = true)] public string? Tags { get; set; } = "";
    [BindProperty] public string? Delete { get; set; }

    public async Task<IActionResult> OnGet([FromRoute] int recordEntryId)
    {
        logger.LogInformation("Viewing record entry {RecordEntryId}", recordEntryId);
        var userAccount = await userAccountRepository.GetUserAccountAsync(User);
        var recordEntry = await recordRepository.GetByIdAsync(userAccount, recordEntryId);
        if (recordEntry == null)
        {
            logger.LogInformation("UserAccount {UserAccountId} can't access record entry id {RecordEntryId} (or doesn't exist)", userAccount.UserAccountId, recordEntry?.RecordEntryId);
            return NotFound();
        }

        EntryId = recordEntry.RecordEntryId;
        EntryDate = recordEntry.EntryDate.ToDisplayString();
        Title = recordEntry.Title;
        Description = recordEntry.Description;
        RemindDate = recordEntry.ReminderDate.ToDisplayString();
        Tags = string.Join(' ', recordEntry.ActiveRecordEntryTags.Select(ret => ret.Tag));

        return Page();
    }

    public async Task<IActionResult> OnPost([FromRoute] int recordEntryId)
    {
        logger.LogInformation("Saving / Deleting record entry {RecordEntryId} (Delete={Delete})", recordEntryId, Delete);
        var userAccount = await userAccountRepository.GetUserAccountAsync(User);
        var recordEntry = await recordRepository.GetByIdAsync(userAccount, recordEntryId);
        if (recordEntry == null)
        {
            logger.LogInformation("UserAccount {UserAccountId} can't access record entry id {RecordEntryId} (or doesn't exist)", userAccount.UserAccountId, recordEntry?.RecordEntryId);
            return NotFound();
        }
        if (string.IsNullOrWhiteSpace(Title))
        {
            logger.LogInformation("Required field Title is null or blank");
            return BadRequest();
        }

        if (!string.IsNullOrEmpty(Delete))
        {
            logger.LogDebug("Deleting record entry {RecordEntryId}", recordEntry.RecordEntryId);
            await recordRepository.DeleteAsync(userAccount, recordEntry);
            return Redirect("/");
        }

        recordEntry.EntryDate = EntryDate.ParseDateOnly();
        recordEntry.Title = Title;
        recordEntry.Description = Description;
        recordEntry.ReminderDate = RemindDate.ParseDateOnly(null);

        logger.LogDebug("Saving record entry {RecordEntryId} with entry date [{EntryDate}], title [{Title}], description [{Description}], reminder date [{RemindDate}, tags [{Tags}]",
            recordEntry.RecordEntryId, recordEntry.EntryDate, recordEntry.Title, recordEntry.Description, recordEntry.ReminderDate, Tags);
        await recordRepository.SaveAsync(userAccount, recordEntry, Tags);

        return Redirect($"/record/{recordEntryId}");
    }
}
