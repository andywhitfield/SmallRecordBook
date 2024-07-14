using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SmallRecordBook.Web.Tests;

public class TestStubAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
        Task.FromResult(Request.Headers.Authorization.Count != 0
            ? AuthenticateResult.Success(
                new AuthenticationTicket(
                    new(new ClaimsIdentity([new(ClaimTypes.GivenName, "Test user"), new(ClaimTypes.Name, "test-user-1")], "Test")),
                    "Test"
                )
            )
            : AuthenticateResult.Fail("No auth provided"));
}