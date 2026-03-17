using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Pages.Shared;
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
    public Pagination Pagination { get; private set; } = Pagination.Empty;
    public string AmountTotal { get; private set; } = "";
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);

        if (View != null)
            await userAccountRepository.SetUserAccountSettingAsync(user, UserAccountSetting.ViewListOrCalendar, View);
        else
            View = await userAccountRepository.GetUserAccountSettingOrDefaultAsync(user, UserAccountSetting.ViewListOrCalendar, "");

        if (View != "calendar") // the calendar view is populated dynamically / via the CalendarController api call
            RecordEntries = LoadRecordEntries(user);

        IEnumerable<RecordEntry> allEntries = RecordEntries.ToImmutableList();
        var pagination = Pagination.Paginate(allEntries, PageNumber);
        RecordEntries = pagination.Items;
        Pagination = new(pagination.Page, pagination.PageCount, Tag, Link, Find);

        if (Link != null || !string.IsNullOrEmpty(Tag) || !string.IsNullOrEmpty(Find))
        {
            // as we haven't loaded any entries when in calendar view, we need to load them now to be able to calculate the totals
            // perhaps not ideal, but good enough for now
            if (View == "calendar")
                allEntries = LoadRecordEntries(user);

            AmountTotal = string.Join("; ", allEntries
                .Where(e => e.Currency != null && e.Amount != null)
                .GroupBy(e => e.Currency)
                .OrderBy(g => g.Key)
                .Select(g => $"{g.Key}{g.Sum(e => e.Amount).FormattedAmount()}"));
        }
    }

    private IEnumerable<RecordEntry> LoadRecordEntries(UserAccount user)
    {
        if (Link != null)
        {
            return recordRepository
                .GetBy(user, re => re.LinkReference == Link)
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        
        if (!string.IsNullOrEmpty(Tag))
        {
            return recordRepository
                .GetBy(user, re => re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && ret.Tag == Tag))
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        
        if (!string.IsNullOrEmpty(Find))
        {
            var like = $"%{Find.Trim()}%";
            return recordRepository
                .GetBy(user, re =>
                    (re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && EF.Functions.Like(ret.Tag, like))) ||
                    EF.Functions.Like(re.Title, like) ||
                    (re.Description != null && EF.Functions.Like(re.Description, like)))
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        
        return recordRepository.GetAll(user);
    }
}
