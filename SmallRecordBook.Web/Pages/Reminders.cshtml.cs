using System.Collections.Immutable;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages;

public class RemindersModel(
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository
)
    : PageModel
{
    public IEnumerable<RecordEntry> RecordEntries { get; private set; } = [];
    public Pagination Pagination { get; private set; } = Pagination.Empty;
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public string View { get; set; } = "upcoming";
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "bydatedesc";

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var query = recordRepository.GetBy(user, re => re.ReminderDate != null && (re.ReminderDone == null || !re.ReminderDone.Value));

        if (View == "upcoming")
            query = query.Where(re => re.ReminderDate <= DateOnly.FromDateTime(DateTime.Today.AddMonths(1)));

        if (Sort == "bydatedesc")
            query = query.OrderBy(re => re.ReminderDate);
        else
            query = query.OrderByDescending(re => re.ReminderDate);
        
        var pagination = Pagination.Paginate(query.ToImmutableList(), PageNumber);
        RecordEntries = pagination.Items;
        Pagination = new(pagination.Page, pagination.PageCount, null, null, null);
    }
}
