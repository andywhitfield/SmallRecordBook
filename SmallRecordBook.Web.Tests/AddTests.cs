using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Repositories;
using Xunit;

namespace SmallRecordBook.Web.Tests;

public class AddTests : IAsyncLifetime
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    public async Task InitializeAsync()
    {
        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.Migrate();
        var userAccount = context.UserAccounts.Add(new UserAccount { Email = "test-user-1" });
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Should_show_add_new_record_form()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/add");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Add new record entry", responseContent);
    }

    [Fact]
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
        Assert.Equal(HttpStatusCode.Redirect, responsePost.StatusCode);
        Assert.Equal("./", responsePost.Headers.Location?.OriginalString);

        await using var serviceScope = _webApplicationFactory.Services.CreateAsyncScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        Assert.Equal(1, context.RecordEntries.Count());
        Assert.Equal(new DateOnly(2024, 7, 14), context.RecordEntries.Single().EntryDate);
        Assert.Equal("New record entry", context.RecordEntries.Single().Title);
        Assert.Equal(DateTime.UtcNow, context.RecordEntries.Single().CreatedDateTime, TimeSpan.FromSeconds(1));
        Assert.Equal("", context.RecordEntries.Single().Description);
        Assert.Null(context.RecordEntries.Single().ReminderDate);
        Assert.Null(context.RecordEntries.Single().DeletedDateTime);
    }

    public Task DisposeAsync()
    {
        _webApplicationFactory.Dispose();
        return Task.CompletedTask;
    }
}
