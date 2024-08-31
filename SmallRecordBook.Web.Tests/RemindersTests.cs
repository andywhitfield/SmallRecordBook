using System.Net;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class RemindersTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Should_show_only_item_with_reminder()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item with reminder",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5)
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item without reminder"
            });
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/reminders");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item with reminder"));
        StringAssert.DoesNotMatch(responseContent, new("Item without reminder"));
    }

    [TestMethod]
    public async Task Should_not_show_done_reminder()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item with reminder done",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5),
                ReminderDone = true
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item with reminder to do",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5)
            });
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/reminders");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item with reminder"));
        StringAssert.DoesNotMatch(responseContent, new("Item without reminder"));
    }

    [TestMethod]
    public async Task Should_only_show_upcoming_reminder()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item with reminder too far in the future",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(40)
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item with upcoming reminder",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5)
            });
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/reminders");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item with upcoming reminder"));
        StringAssert.DoesNotMatch(responseContent, new("Item with reminder too far in the future"));
    }

    [TestMethod]
    public async Task Should_show_future_reminders_when_requested()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item with reminder too far in the future",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(40)
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item with upcoming reminder",
                ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(5)
            });
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/reminders?view=all");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item with upcoming reminder"));
        StringAssert.Matches(responseContent, new("Item with reminder too far in the future"));
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
