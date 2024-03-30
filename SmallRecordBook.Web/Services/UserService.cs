using System.Security.Claims;

namespace SmallRecordBook.Web.Services;

public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public bool IsLoggedIn => !string.IsNullOrEmpty(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name));
}