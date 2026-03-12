using System.Net;
using Microsoft.EntityFrameworkCore;
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
        Assert.Contains("class=\"srb-entry-currency\" value=\"&#xA3;\"", responseContent, message: "Default currency should be £ when there is no entry with a currency");
    }

    [TestMethod]
    public async Task Should_default_to_most_recent_currency_When_showing_the_new_record_form()
    {
        await ArrangeAsync();

        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/add");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("class=\"srb-entry-currency\" value=\"&#xA5;\"", responseContent);

        async Task ArrangeAsync()
        {
            await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
            using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
            var allUsers = await context.UserAccounts.ToListAsync();
            var testUser = await TestStubAuthHandler.GetTestUserAsync(context);
            var otherUser = context.UserAccounts.Add(new() { Email = "test-other-user" });
            context.RecordEntries.AddRange([
                new() {
                    UserAccount = testUser,
                    EntryDate = new(2026, 3, 12),
                    Title = "test entry 1",
                    Currency = "€",
                    Amount = 1,
                    CreatedDateTime = DateTime.UtcNow.AddMinutes(-5)
                },
                new() {
                    UserAccount = testUser,
                    EntryDate = new(2026, 3, 12),
                    Title = "test entry 2",
                    Currency = "¥",
                    Amount = 2,
                    CreatedDateTime = DateTime.UtcNow.AddMinutes(-2)
                },
                new() {
                    UserAccount = testUser,
                    EntryDate = new(2026, 3, 12),
                    Title = "test entry 3",
                    Currency = "$",
                    Amount = 3,
                    CreatedDateTime = DateTime.UtcNow.AddMinutes(-10)
                },
                new() {
                    UserAccount = otherUser.Entity,
                    EntryDate = new(2026, 3, 12),
                    Title = "test entry 4",
                    Currency = "£",
                    Amount = 4,
                    CreatedDateTime = DateTime.UtcNow.AddMinutes(-1)
                }
            ]);
            await context.SaveChangesAsync();
        }
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
            { "Currency", "£" },
            { "Amount", "10.99" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(addPage, "/add") }
        }));
        Assert.AreEqual(HttpStatusCode.Redirect, responsePost.StatusCode);
        Assert.AreEqual("/", responsePost.Headers.Location?.OriginalString);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.AreEqual(1, context.RecordEntries.Count());
        Assert.AreEqual(new DateOnly(2024, 7, 14), context.RecordEntries.Single().EntryDate);
        Assert.AreEqual("New record entry", context.RecordEntries.Single().Title);
        Assert.AreEqual("£", context.RecordEntries.Single().Currency);
        Assert.AreEqual(10.99m, context.RecordEntries.Single().Amount);
        Assert.AreEqual(DateTime.UtcNow.Ticks, context.RecordEntries.Single().CreatedDateTime.Ticks, TimeSpan.FromSeconds(1).Ticks);
        Assert.AreEqual("", context.RecordEntries.Single().Description);
        Assert.IsNull(context.RecordEntries.Single().ReminderDate);
        Assert.IsNull(context.RecordEntries.Single().DeletedDateTime);
    }

    [TestMethod]
    public async Task When_no_amount_is_entered_Then_currency_should_be_null()
    {
        using var client = _webApplicationFactory.CreateClient(true, false);
        using var responseGet = await client.GetAsync("/add");
        var addPage = await responseGet.Content.ReadAsStringAsync();
        using var responsePost = await client.PostAsync("/add", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "EntryDate", "2026-03-12" },
            { "Title", "New record entry" },
            { "Currency", "" },
            { "Amount", "" },
            { "__RequestVerificationToken", WebApplicationFactoryTest.GetFormValidationToken(addPage, "/add") }
        }));
        Assert.AreEqual(HttpStatusCode.Redirect, responsePost.StatusCode);
        Assert.AreEqual("/", responsePost.Headers.Location?.OriginalString);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.AreEqual(1, context.RecordEntries.Count());
        Assert.AreEqual(new DateOnly(2026, 3, 12), context.RecordEntries.Single().EntryDate);
        Assert.AreEqual("New record entry", context.RecordEntries.Single().Title);
        Assert.IsNull(context.RecordEntries.Single().Currency);
        Assert.IsNull(context.RecordEntries.Single().Amount);
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
