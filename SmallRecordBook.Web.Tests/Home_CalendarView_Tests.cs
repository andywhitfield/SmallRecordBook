using System.Net;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class Home_CalendarView_Tests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Should_show_calendar_control_and_no_events()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 description"
            });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/?view=calendar");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.DoesNotMatch(responseContent, new("Item1"));
        StringAssert.Contains(responseContent, "FullCalendar.Calendar");
        StringAssert.Contains(responseContent, "Logout");
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
