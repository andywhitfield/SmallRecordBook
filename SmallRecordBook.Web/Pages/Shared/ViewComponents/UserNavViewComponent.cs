using Microsoft.AspNetCore.Mvc;
using SmallRecordBook.Web.Pages.ViewModels;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages.Shared.ViewComponents;

public class UserNavViewComponent(
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await userAccountRepository.GetUserAccountAsync(UserClaimsPrincipal);
        return View(new UserNavModel(
            recordRepository.GetTags(user).Take(8),
            recordRepository.GetBy(user, re => (re.ReminderDone == null || !re.ReminderDone.Value) && re.ReminderDate != null && re.ReminderDate.Value < DateOnly.FromDateTime(DateTime.UtcNow)).Count(),
            Request));
    }
}