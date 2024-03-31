using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace SmallRecordBook.Web.Tests;

public class WebApplicationFactoryTest : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services => services
            .AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestStubAuthHandler>("Test", null));
    

    public HttpClient CreateClient(bool isAuthorized)
    {
        var client = CreateClient();
        if (isAuthorized)
            client.DefaultRequestHeaders.Authorization = new("Test");
        return client;
    }
}
