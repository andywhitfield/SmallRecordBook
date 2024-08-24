using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class AddTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Should_show_add_new_record_form()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/add");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Add new record entry");
    }

    [TestMethod]
    public async Task Given_completed_form_should_show_add_new_record_and_redirect_to_home_page()
    {
        using var client = _webApplicationFactory.CreateClient(true, false);
        using var responseGet = await client.GetAsync("/add");
        var addPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync("/add", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", "2024-07-14" },
            { "Title", "New record entry" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(addPage, "/add") }
        }));
        Assert.AreEqual(HttpStatusCode.Redirect, responsePost.StatusCode);
        Assert.AreEqual("./", responsePost.Headers.Location?.OriginalString);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.AreEqual(1, context.RecordEntries.Count());
        Assert.AreEqual(new DateOnly(2024, 7, 14), context.RecordEntries.Single().EntryDate);
        Assert.AreEqual("New record entry", context.RecordEntries.Single().Title);
        Assert.AreEqual(DateTime.UtcNow.Ticks, context.RecordEntries.Single().CreatedDateTime.Ticks, TimeSpan.FromSeconds(1).Ticks);
        Assert.AreEqual("", context.RecordEntries.Single().Description);
        Assert.IsNull(context.RecordEntries.Single().ReminderDate);
        Assert.IsNull(context.RecordEntries.Single().DeletedDateTime);
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
