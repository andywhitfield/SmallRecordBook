using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Tests;

public class WebApplicationFactoryTest : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SqliteDataContext> _options;

    public WebApplicationFactoryTest()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<SqliteDataContext>().UseSqlite(_connection).Options;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services => services
            .Replace(ServiceDescriptor.Scoped(_ => new SqliteDataContext(_options)))
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestStubAuthHandler>("Test", null));


    public HttpClient CreateClient(bool isAuthorized, bool allowAutoRedirect = true)
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect });
        if (isAuthorized)
            client.DefaultRequestHeaders.Authorization = new("Test");
        return client;
    }

    public async Task AddRecordEntriesAsync(params RecordEntry[] recordEntries)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.UserAccounts.AttachRange(recordEntries.Select(re => re.UserAccount).Where(ua => ua.UserAccountId > 0));
        context.RecordEntries.AddRange(recordEntries);
        await context.SaveChangesAsync();
    }

    public async Task AddRecordEntryAsync(RecordEntry recordEntry, params string[] tags)
    {
        await using var scope = Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        if (recordEntry.UserAccount.UserAccountId > 0)
            context.UserAccounts.Attach(recordEntry.UserAccount);
        context.RecordEntries.Add(recordEntry);
        context.RecordEntryTags.AddRange(tags.Select(t => new RecordEntryTag
        {
            RecordEntry = recordEntry,
            Tag = t
        }));
        await context.SaveChangesAsync();
    }

    public static string GetFormValidationToken(string responseContent, string formAction)
    {
        var validationToken = responseContent[responseContent.IndexOf($"action=\"{formAction}\"")..];
        validationToken = validationToken[validationToken.IndexOf("__RequestVerificationToken")..];
        validationToken = validationToken[(validationToken.IndexOf("value=\"") + 7)..];
        validationToken = validationToken[..validationToken.IndexOf('"')];
        return validationToken;
    }
}
