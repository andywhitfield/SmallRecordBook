using Microsoft.AspNetCore.Mvc;
using SmallRecordBook.Web.Pages.ViewModels;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Pages.Shared.ViewComponents;

public class UserTagListViewComponent(
    IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository)
    : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var user = await userAccountRepository.GetUserAccountAsync(UserClaimsPrincipal);
        return View(new UserTagListModel(recordRepository.GetTags(user)));
    }
}