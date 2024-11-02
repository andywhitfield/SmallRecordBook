using System.Net;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class Home_Pagination_Tests
{
    private const int pageSize = 10; // TODO: this could/should be a user-settable property, or at least configurable

    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    [DataRow(pageSize)]
    [DataRow(pageSize - 1)]
    [DataRow(1)]
    public async Task Show_all_items(int size)
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            Enumerable.Range(1, size).Select(num =>
            new RecordEntry
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item" + num + "Title",
                UserAccount = testUser
            }).ToArray());
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        foreach (var num in Enumerable.Range(1, size))
            StringAssert.Matches(responseContent, new("Item" + num + "Title"));
    }

    [TestMethod]
    [DataRow(pageSize + 1)]
    [DataRow(pageSize * 3)]
    public async Task Show_page_1_items_only(int size)
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            Enumerable.Range(1, size).Select(num =>
            new RecordEntry
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-num)),
                Title = "Item" + num + "Title",
                UserAccount = testUser
            }).ToArray());
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        foreach (var num in Enumerable.Range(1, pageSize))
            StringAssert.Matches(responseContent, new("Item" + num + "Title"));
        foreach (var num in Enumerable.Range(pageSize + 1, size - pageSize))
            StringAssert.DoesNotMatch(responseContent, new("Item" + num + "Title"));
    }

    [TestMethod]
    [DataRow(pageSize + 1)]
    [DataRow(pageSize * 2)]
    [DataRow((pageSize * 2) + 1)]
    [DataRow(pageSize * 3)]
    public async Task Show_page_2_items_only(int size)
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            Enumerable.Range(1, size).Select(num =>
            new RecordEntry
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-num)),
                Title = "Item" + num + "Title",
                UserAccount = testUser
            }).ToArray());
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/?pageNumber=2");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        foreach (var num in Enumerable.Range(1, pageSize))
            StringAssert.DoesNotMatch(responseContent, new("Item" + num + "Title"));
        foreach (var num in Enumerable.Range(pageSize + 1, Math.Min(size - pageSize, pageSize)))
            StringAssert.Matches(responseContent, new("Item" + num + "Title"));
        foreach (var num in Enumerable.Range((pageSize * 2) + 1, Math.Max(0, size - (pageSize * 2))))
            StringAssert.DoesNotMatch(responseContent, new("Item" + num + "Title"));
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
