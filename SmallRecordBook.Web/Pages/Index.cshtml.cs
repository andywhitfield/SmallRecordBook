using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages;

public class IndexModel(
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository
)
    : PageModel
{
    [BindProperty(SupportsGet = true)] public Guid? Link { get; set; }
    [BindProperty(SupportsGet = true)] public string? Tag { get; set; }
    [BindProperty(SupportsGet = true)] public string? Find { get; set; }
    [BindProperty(SupportsGet = true)] public string? View { get; set; }
    public IEnumerable<RecordEntry> RecordEntries { get; private set; } = [];

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);

        if (View != null)
            await userAccountRepository.SetUserAccountSettingAsync(user, UserAccountSetting.ViewListOrCalendar, View);
        else
            View = await userAccountRepository.GetUserAccountSettingOrDefaultAsync(user, UserAccountSetting.ViewListOrCalendar, "");

        if (Link != null)
        {
            RecordEntries = recordRepository
                .GetBy(user, re => re.LinkReference == Link)
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        else if (!string.IsNullOrEmpty(Tag))
        {
            RecordEntries = recordRepository
                .GetBy(user, re => re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && ret.Tag == Tag))
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        else if (!string.IsNullOrEmpty(Find))
        {
            var like = $"%{Find.Trim()}%";
            RecordEntries = recordRepository
                .GetBy(user, re =>
                    (re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && EF.Functions.Like(ret.Tag, like))) ||
                    EF.Functions.Like(re.Title, like) ||
                    (re.Description != null && EF.Functions.Like(re.Description, like)))
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        else
        {
            RecordEntries = recordRepository.GetAll(user);
        }

        RecordEntries = RecordEntries.ToImmutableList();
    }
}
