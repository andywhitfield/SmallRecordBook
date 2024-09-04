using Microsoft.AspNetCore.Mvc.RazorPages;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages;

public class TagsModel(
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository
)
    : PageModel
{
    public IEnumerable<(string Tag, int TagCount)> Tags { get; private set; } = [];

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        Tags = recordRepository.GetTags(user);
    }
}
