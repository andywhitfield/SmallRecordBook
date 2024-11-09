using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public interface IUserFeedRepository
{
    Task<UserFeed?> GetAsync(int userFeedId);
    Task<List<UserFeed>> GetAsync(UserAccount user);
    Task CreateAsync(UserAccount user, string uniqueFeedIdentifier);
    Task SaveAsync(UserFeed userFeed);
    Task<UserFeed?> FindByIdentifierAsync(string feedIdentifier);
}