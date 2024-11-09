using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages;

public class ProfileModel(
    ILogger<ProfileModel> logger,
    IUserAccountRepository userAccountRepository,
    IUserFeedRepository userFeedRepository
)
    : PageModel
{
    public UserFeed? Feed { get; private set; }

    public async Task OnGet()
    {
        var user = await userAccountRepository.GetUserAccountAsync(User);
        Feed = (await userFeedRepository.GetAsync(user)).FirstOrDefault();
    }

    public async Task<IActionResult> OnPostCreate()
    {
        logger.LogInformation("Creating new feed");
        var user = await userAccountRepository.GetUserAccountAsync(User);
        await userFeedRepository.CreateAsync(user, NewGuidString());
        return Redirect("/profile");
    }

    public async Task<IActionResult> OnPostDelete()
    {
        logger.LogInformation("Deleting feed");

        var user = await userAccountRepository.GetUserAccountAsync(User);
        foreach (var userFeed in await userFeedRepository.GetAsync(user))
        {
            userFeed.DeletedDateTime = DateTime.UtcNow;
            await userFeedRepository.SaveAsync(userFeed);
        }

        return Redirect("/profile");
    }

    private static string NewGuidString()
    {
        var base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace('+', '-').Replace('/', '_');
        return base64Guid.Substring(0, base64Guid.Length - 2);
    }
}
