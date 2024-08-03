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
    public IEnumerable<string> Tags { get; private set; } = [];
    public IEnumerable<RecordEntry> RecordEntries { get; private set; } = [];

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        Tags = recordRepository.GetTags(user);
        RecordEntries = recordRepository.GetAll(user);
    }
}
