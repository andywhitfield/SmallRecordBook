using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Tests;

public class TestStubAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string _testUserEmail = "test-user-1";

    public static async Task AddTestUserAsync(IServiceProvider serviceProvider)
    {
        await using var serviceScope = serviceProvider.CreateAsyncScope();
        using var context = serviceScope.ServiceProvider.GetRequiredService<SqliteDataContext>();
        context.Migrate();
        var userAccount = context.UserAccounts.Add(new() { Email = _testUserEmail });
        await context.SaveChangesAsync();
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult(Request.Headers.Authorization.Count != 0
            ? AuthenticateResult.Success(
                new AuthenticationTicket(
                    new(new ClaimsIdentity([new(ClaimTypes.GivenName, "Test user"), new(ClaimTypes.Name, _testUserEmail)], "Test")),
                    "Test"
                )
            )
            : AuthenticateResult.Fail("No auth provided"));
}