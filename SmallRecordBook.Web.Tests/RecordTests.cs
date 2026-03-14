using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class RecordTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public async Task InitializeAsync()
    {
        await TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Currency = "£",
                Amount = 10.99m,
                Description = "Item1 in pounds"
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Currency = "$",
                Amount = 12,
                Description = "Item2 in dollars, with no decimal places"
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser,
                Currency = "£",
                Amount = 5.1m
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 with no amount"
            });
    }

    [TestMethod]
    public async Task Should_display_item1()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/record/{await RecordIdAsync("Item1")}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("value=\"&#xA3;\"", responseContent);
        Assert.Contains("value=\"10.99\"", responseContent);
        Assert.Contains("value=\"Item1\"", responseContent);
        Assert.Contains(">Item1 in pounds<", responseContent);
    }

    [TestMethod]
    public async Task Should_display_item2()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/record/{await RecordIdAsync("Item2")}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("value=\"$\"", responseContent);
        Assert.Contains("value=\"12\"", responseContent);
        Assert.Contains("value=\"Item2\"", responseContent);
        Assert.Contains(">Item2 in dollars, with no decimal places<", responseContent);
    }

    [TestMethod]
    public async Task Should_display_item3()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/record/{await RecordIdAsync("Item3")}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("value=\"&#xA3;\"", responseContent);
        Assert.Contains("value=\"5.10\"", responseContent);
        Assert.Contains("value=\"Item3\"", responseContent);
    }

    [TestMethod]
    public async Task Should_display_item4()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/record/{await RecordIdAsync("Item4")}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("value=\"Item4\"", responseContent);
        Assert.Contains(">Item4 with no amount<", responseContent);
    }

    private async Task<int> RecordIdAsync(string title)
    {
        await using var scope =  _webApplicationFactory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IRecordRepository>().GetBy(await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services), re => re.Title == title).Select(re => re.RecordEntryId).SingleAsync();
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
