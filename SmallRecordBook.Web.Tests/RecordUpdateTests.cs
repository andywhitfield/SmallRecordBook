using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmallRecordBook.Web.Models;
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

    [TestMethod]
    public async Task Check_title_is_mandatory()
    {
        using var client = _webApplicationFactory.CreateClient(true, false);
        var pageUri = $"/record/{await RecordIdAsync()}";
        using var responseGet = await client.GetAsync(pageUri);
        var recordViewPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync(pageUri, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") },
            { "Title", "" },
            { "Currency", "£" },
            { "Amount", "10.99" },
            { "Description", "Item1 in pounds" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(recordViewPage, pageUri) }
        }));
        Assert.AreEqual(HttpStatusCode.BadRequest, responsePost.StatusCode);
    }

    [TestMethod]
    public async Task Can_update_amount()
    {
        using var client = _webApplicationFactory.CreateClient(true, false);
        var pageUri = $"/record/{await RecordIdAsync()}";
        using var responseGet = await client.GetAsync(pageUri);
        var recordViewPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync(pageUri, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") },
            { "Title", "Item1" },
            { "Currency", "$" },
            { "Amount", "1000.9" },
            { "Description", "Item1 in dollars" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(recordViewPage, pageUri) }
        }));
        Assert.AreEqual(HttpStatusCode.Found, responsePost.StatusCode);
        Assert.AreEqual(pageUri, responsePost.Headers.Location?.OriginalString);
        using var responsePostUpdate = await client.GetAsync(pageUri);
        var updatedRecordPage = await responsePostUpdate.Content.ReadAsStringAsync();
        Assert.Contains("value=\"Item1\"", updatedRecordPage);
        Assert.Contains("value=\"1000.90\"", updatedRecordPage);
        Assert.Contains("value=\"$\"", updatedRecordPage);
        Assert.DoesNotContain("value=\"£\"", updatedRecordPage);
        Assert.DoesNotContain("value=\"&#xA3\"", updatedRecordPage);
        Assert.Contains(">Item1 in dollars<", updatedRecordPage);
        Assert.DoesNotContain(">Item1 in pounds<", updatedRecordPage);
    }

    [TestMethod]
    public async Task Can_mark_reminder_as_done()
    {
        var recordId = await RecordIdAsync();
        await UpdateRecordAsync(recordId, re =>
        {
            re.ReminderDate = DateOnly.FromDateTime(DateTime.UtcNow);
            re.ReminderDone = false;
        });
        using var client = _webApplicationFactory.CreateClient(true, false);
        var pageUri = $"/record/{recordId}";
        using var responseGet = await client.GetAsync(pageUri);
        var recordViewPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync(pageUri, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") },
            { "Title", "Item1" },
            { "Currency", "£" },
            { "Amount", "10.99" },
            { "Description", "Item1 in pounds" },
            { "RemindDate", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") },
            { "RemindDone", "true" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(recordViewPage, pageUri) }
        }));
        Assert.AreEqual(HttpStatusCode.Found, responsePost.StatusCode);
        Assert.AreEqual(pageUri, responsePost.Headers.Location?.OriginalString);

        await using var scope =  _webApplicationFactory.Services.CreateAsyncScope();
        var recordEntry = scope.ServiceProvider.GetRequiredService<SqliteDataContext>().RecordEntries.Find(recordId);
        Assert.IsNotNull(recordEntry);
        Assert.IsTrue(recordEntry.ReminderDone);
        Assert.AreEqual(DateOnly.FromDateTime(DateTime.UtcNow), recordEntry.ReminderDate);
    }

    [TestMethod]
    public async Task Can_delete()
    {
        var recordId = await RecordIdAsync();
        using var client = _webApplicationFactory.CreateClient(true, false);
        var pageUri = $"/record/{recordId}";
        using var responseGet = await client.GetAsync(pageUri);
        var recordViewPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync(pageUri, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd") },
            { "Title", "Item1" },
            { "Currency", "£" },
            { "Amount", "10.99" },
            { "Description", "Item1 in pounds" },
            { "Delete", "delete" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(recordViewPage, pageUri) }
        }));
        Assert.AreEqual(HttpStatusCode.Found, responsePost.StatusCode);
        Assert.AreEqual("/", responsePost.Headers.Location?.OriginalString);

        using var responsePostUpdate = await client.GetAsync("/");
        var homePage = await responsePostUpdate.Content.ReadAsStringAsync();
        Assert.DoesNotContain("Item1", homePage);

        await using var scope =  _webApplicationFactory.Services.CreateAsyncScope();
        var recordEntry = scope.ServiceProvider.GetRequiredService<SqliteDataContext>().RecordEntries.Find(recordId);
        Assert.IsNotNull(recordEntry);
        Assert.IsNotNull(recordEntry.DeletedDateTime);
    }

    private async Task<int> RecordIdAsync()
    {
        await using var scope =  _webApplicationFactory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IRecordRepository>().GetBy(await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services), re => re.Title == "Item1").Select(re => re.RecordEntryId).SingleAsync();
    }

    private async Task UpdateRecordAsync(int recordId, Action<RecordEntry> applyChanges)
    {
        await using var scope =  _webApplicationFactory.Services.CreateAsyncScope();
        var dataContext = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        var recordEntry = dataContext.RecordEntries.Find(recordId) ?? throw new InvalidOperationException($"Couldn't find record entry with id {recordId}");
        applyChanges(recordEntry);
        await dataContext.SaveChangesAsync();
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
