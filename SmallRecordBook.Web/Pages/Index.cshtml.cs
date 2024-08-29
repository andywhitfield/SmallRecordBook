using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    public IEnumerable<RecordEntry> RecordEntries { get; private set; } = [];

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        if (Link != null)
        {
            RecordEntries = recordRepository
                .GetBy(user, re => re.LinkReference == Link)
                .OrderByDescending(e => e.EntryDate)
                .ThenBy(e => e.Title);
        }
        else
        {
            RecordEntries = recordRepository.GetAll(user);
        }
    }
}
