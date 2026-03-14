using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class RecordUpdateTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public async Task InitializeAsync()
    {
        await TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            [new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Currency = "£",
                Amount = 10.99m,
                Description = "Item1 in pounds"
            }]);
    }

    [TestMethod]
    public async Task Can_update_title()
    {
        using var client = _webApplicationFactory.CreateClient(true, false);
        var pageUri = $"/record/{await RecordIdAsync()}";
        using var responseGet = await client.GetAsync(pageUri);
        var recordViewPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync(pageUri, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") },
            { "Title", "Item1 Updated" },
            { "Currency", "£" },
            { "Amount", "10.99" },
            { "Description", "Item1 in pounds" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(recordViewPage, pageUri) }
        }));
        Assert.AreEqual(HttpStatusCode.Found, responsePost.StatusCode);
        Assert.AreEqual(pageUri, responsePost.Headers.Location?.OriginalString);
        using var responsePostUpdate = await client.GetAsync(pageUri);
        var updatedRecordPage = await responsePostUpdate.Content.ReadAsStringAsync();
        Assert.Contains("value=\"Item1 Updated\"", updatedRecordPage);
        Assert.DoesNotContain("value=\"Item1\"", updatedRecordPage);
    }

    private async Task<int> RecordIdAsync()
    {
        await using var scope =  _webApplicationFactory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IRecordRepository>().GetBy(await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services), re => re.Title == "Item1").Select(re => re.RecordEntryId).SingleAsync();
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
