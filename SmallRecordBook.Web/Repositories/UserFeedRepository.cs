using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class UserFeedRepository(SqliteDataContext context, ILogger<UserFeedRepository> logger)
    : IUserFeedRepository
{
    public Task CreateAsync(UserAccount user, string uniqueFeedIdentifier)
    {
        logger.LogInformation("Creating new feed for user {userAccountId}, {uniqueFeedIdentifier}", user.UserAccountId, uniqueFeedIdentifier);

        context.UserFeeds.Add(new()
        {
            UserAccount = user,
            UserFeedIdentifier = uniqueFeedIdentifier
        });

        return context.SaveChangesAsync();
    }

    public Task<UserFeed?> FindByIdentifierAsync(string feedIdentifier) =>
        context.UserFeeds.SingleOrDefaultAsync(f => f.UserFeedIdentifier == feedIdentifier && f.DeletedDateTime == null);

    public Task<UserFeed?> GetAsync(int userFeedId) =>
        context.UserFeeds.SingleOrDefaultAsync(f => f.UserFeedId == userFeedId && f.DeletedDateTime == null);

    public Task<List<UserFeed>> GetAsync(UserAccount user)=>
        context.UserFeeds.Where(f => f.UserAccount == user && f.DeletedDateTime == null).OrderByDescending(f => f.CreatedDateTime).ToListAsync();

    public Task SaveAsync(UserFeed userFeed)
    {
        userFeed.LastUpdateDateTime = DateTime.UtcNow;
        return context.SaveChangesAsync();
    }
}