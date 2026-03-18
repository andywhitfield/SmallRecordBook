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
                Description = "Item1 description",
                Currency = "£",
                Amount = 5.1m
            });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/?view=calendar");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.DoesNotMatch(responseContent, new("Item1"));
        StringAssert.Contains(responseContent, "FullCalendar.Calendar");
        StringAssert.Contains(responseContent, "Logout");
        Assert.DoesNotContain("Total amount:", responseContent, message: "Total amount should not be shown when listing all items");
    }

    [TestMethod]
    public async Task Should_show_total_amount_when_viewing_calendar_by_tag()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 description",
                Currency = "£",
                Amount = 5.1m
            }, "tag1");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 description",
                Currency = "$",
                Amount = 6.2m
            }, "tag2");

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/?view=calendar&tag=tag2");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Item1", responseContent);
        Assert.DoesNotContain("Item2", responseContent);
        Assert.Contains("FullCalendar.Calendar", responseContent);
        Assert.Contains(">Amount total: $6.20<", responseContent);
    }

    [TestMethod]
    public async Task Should_show_total_amount_when_viewing_calendar_by_linked_item()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        var link1 = Guid.NewGuid();
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 description",
                Currency = "£",
                Amount = 5.1m,
                LinkReference = link1
            });
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 description",
                Currency = "$",
                Amount = 6.2m,
                LinkReference = Guid.NewGuid()
            });
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser,
                Description = "Item3 description",
                Currency = "$",
                Amount = 6000m,
                LinkReference = link1
            });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/?view=calendar&link={link1}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Item1", responseContent);
        Assert.DoesNotContain("Item2", responseContent);
        Assert.Contains("FullCalendar.Calendar", responseContent);
        Assert.Contains(">Amount total: $6,000; &#xA3;5.10<", responseContent);
    }

    [TestMethod]
    public async Task Should_show_total_amount_when_viewing_calendar_by_find()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 description find this",
                Currency = "£",
                Amount = 5.1m
            });
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 description",
                Currency = "$",
                Amount = 6.2m
            });
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser,
                Description = "Item3 description find this",
                Currency = "£",
                Amount = 6000m
            });

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/?view=calendar&find=find+this");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Item1", responseContent);
        Assert.DoesNotContain("Item2", responseContent);
        Assert.Contains("FullCalendar.Calendar", responseContent);
        Assert.Contains(">Amount total: &#xA3;6,005.10<", responseContent);
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
