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

    public async Task<IActionResult> OnGet([FromRoute] int recordEntryId)
    {
        logger.LogInformation("Viewing record entry {RecordEntryId}", recordEntryId);
        var userAccount = await userAccountRepository.GetUserAccountAsync(User);
        var recordEntry = await recordRepository.GetByIdAsync(recordEntryId);
        if (recordEntry?.UserAccountId != userAccount.UserAccountId)
        {
            logger.LogInformation("UserAccount {UserAccountId} can't access record entry id {RecordEntryId} (or doesn't exist)", userAccount.UserAccountId, recordEntry?.RecordEntryId);
            return NotFound();
        }

        EntryId = recordEntry.RecordEntryId;
        EntryDate = recordEntry.EntryDate.ToDisplayString();
        Title = recordEntry.Title;
        Description = recordEntry.Description;
        RemindDate = recordEntry.ReminderDate.ToDisplayString();
        Tags = string.Join(' ', recordEntry.RecordEntryTags?.Select(ret => ret.Tag).OrderBy(x => x).AsEnumerable() ?? []);

        return Page();
    }
}
