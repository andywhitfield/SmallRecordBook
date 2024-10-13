using System.Net;
using System.Text.Json;

namespace SmallRecordBook.Web.Tests.Controllers;

[TestClass]
public class CalendarControllerTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Get_entries_for_october()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = new(2024, 10, 13),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 description"
            },
            new()
            {
                EntryDate = new(2024, 9, 1),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 description"
            },
            new()
            {
                EntryDate = new(2024, 8, 31),
                Title = "Item3",
                UserAccount = testUser,
                Description = "Item3 description"
            },
            new()
            {
                EntryDate = new(2024, 11, 30),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 description"
            },
            new()
            {
                EntryDate = new(2024, 12, 1),
                Title = "Item5",
                UserAccount = testUser,
                Description = "Item5 description"
            });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/api/calendar/recordentries?date=2024-10-13");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Item1");
        StringAssert.Contains(responseContent, "Item1 description");

        StringAssert.Contains(responseContent, "Item2");
        StringAssert.Contains(responseContent, "Item2 description");

        StringAssert.DoesNotMatch(responseContent, new("Item3"));
        StringAssert.DoesNotMatch(responseContent, new("Item3 description"));

        StringAssert.Contains(responseContent, "Item4");
        StringAssert.Contains(responseContent, "Item4 description");

        StringAssert.DoesNotMatch(responseContent, new("Item5"));
        StringAssert.DoesNotMatch(responseContent, new("Item5 description"));
    }

    [TestMethod]
    [DataRow("2024-10-13", 2, 2)]
    [DataRow("2024-10-01", 2, 2)]
    [DataRow("2024-09-30", 1, 3)]
    [DataRow("2024-09-01", 1, 3)]
    [DataRow("2024-08-31", 0, 4)]
    [DataRow("2024-08-01", 0, 4)]
    [DataRow("2024-07-31", 0, 5)]
    [DataRow("2024-07-01", 0, 5)]
    [DataRow("2024-10-31", 2, 2)]
    [DataRow("2024-11-01", 3, 1)]
    [DataRow("2024-11-30", 3, 1)]
    [DataRow("2024-12-01", 4, 0)]
    [DataRow("2024-12-31", 4, 0)]
    [DataRow("2025-01-01", 5, 0)]
    [DataRow("2025-01-31", 5, 0)]
    public async Task Get_entry_counts_for_october(string date, int expectedBefore, int expectedAfter)
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = new(2024, 10, 13),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 description"
            },
            new()
            {
                EntryDate = new(2024, 9, 1),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 description"
            },
            new()
            {
                EntryDate = new(2024, 8, 31),
                Title = "Item3",
                UserAccount = testUser,
                Description = "Item3 description"
            },
            new()
            {
                EntryDate = new(2024, 11, 30),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 description"
            },
            new()
            {
                EntryDate = new(2024, 12, 1),
                Title = "Item5",
                UserAccount = testUser,
                Description = "Item5 description"
            });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/api/calendar/recordentrycounts?date={date}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        var responseJson = JsonDocument.Parse(responseContent);
        var eventCountBeforeCurrentMonth = responseJson.RootElement.GetProperty("eventCountBeforeCurrentMonth").GetInt32();
        var eventCountAfterCurrentMonth = responseJson.RootElement.GetProperty("eventCountAfterCurrentMonth").GetInt32();

        Assert.AreEqual(expectedBefore, eventCountBeforeCurrentMonth);
        Assert.AreEqual(expectedAfter, eventCountAfterCurrentMonth);
    }
}