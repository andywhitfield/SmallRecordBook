using System.Net;

namespace SmallRecordBook.Web.Tests.Controllers;

[TestClass]
public class FeedControllerTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Invalid_feed_identifier_returns_notfound()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/api/feed/test-1234");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task Valid_feed_identifier_returns_empty_rss()
    {
        await _webApplicationFactory.ConfigureDbContextAsync(async context =>
        {
            var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
            context.UserAccounts.Attach(testUser);
            context.UserFeeds.Add(new() { UserAccount = testUser, UserFeedIdentifier = "test-1234" });
        });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/api/feed/test-1234");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var feedContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(feedContent, "feed");
        StringAssert.Contains(feedContent, "title");
        StringAssert.DoesNotMatch(feedContent, new("entry"));
    }

    [TestMethod]
    public async Task Should_contain_upcoming_reminders_in_feed()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.ConfigureDbContextAsync(context =>
        {
            context.UserAccounts.Attach(testUser);
            context.UserFeeds.Add(new() { UserAccount = testUser, UserFeedIdentifier = "test-1234" });
            return Task.CompletedTask;
        });
        await _webApplicationFactory.AddRecordEntriesAsync(
            new() { UserAccount = testUser, EntryDate = DateOnly.FromDateTime(DateTime.Today), Title = "entry-1-" },
            new() { UserAccount = testUser, EntryDate = DateOnly.FromDateTime(DateTime.Today), Title = "entry-2-", ReminderDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)) },
            new() { UserAccount = testUser, EntryDate = DateOnly.FromDateTime(DateTime.Today), Title = "entry-3-", ReminderDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) },
            new() { UserAccount = testUser, EntryDate = DateOnly.FromDateTime(DateTime.Today), Title = "entry-4-", ReminderDate = DateOnly.FromDateTime(DateTime.Today) },
            new() { UserAccount = testUser, EntryDate = DateOnly.FromDateTime(DateTime.Today), Title = "entry-5-", ReminderDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1)) },
            new() { UserAccount = testUser, EntryDate = DateOnly.FromDateTime(DateTime.Today), Title = "entry-6-", ReminderDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-7)) }
        );

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/api/feed/test-1234");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var feedContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(feedContent, "feed");
        StringAssert.Contains(feedContent, "title");
        
        StringAssert.DoesNotMatch(feedContent, new("entry-1-"));

        StringAssert.Contains(feedContent, "entry-2-");
        StringAssert.Contains(feedContent, "Reminder upcoming: entry-2-");

        StringAssert.Contains(feedContent, "entry-3-");
        StringAssert.Contains(feedContent, "Reminder upcoming: entry-3-");

        StringAssert.Contains(feedContent, "entry-4-");
        StringAssert.Contains(feedContent, "Reminder due: entry-4-");

        StringAssert.Contains(feedContent, "entry-5-");
        StringAssert.Contains(feedContent, "Reminder overdue: entry-5-");

        StringAssert.Contains(feedContent, "entry-6-");
        StringAssert.Contains(feedContent, "Reminder overdue: entry-6-");
    }
}