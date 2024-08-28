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
    [BindProperty(SupportsGet = true)] public string View { get; set; } = "upcoming";
    [BindProperty(SupportsGet = true)] public string Sort { get; set; } = "bydatedesc";

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        var query = recordRepository.GetBy(user, re => re.ReminderDate != null && !re.ReminderDone.GetValueOrDefault());
        
        if (View == "upcoming")
            query = query.Where(re => re.ReminderDate.GetValueOrDefault() <= DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1)));
        
        if (Sort == "bydatedesc")
            query = query.OrderBy(re => re.ReminderDate);
        else
            query = query.OrderByDescending(re => re.ReminderDate);

        RecordEntries = query;
    }
}
