using System.Net;
using Xunit;

namespace SmallRecordBook.Web.Tests;

public class HomeTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [Fact]
    public async Task Given_no_credentials_should_redirect_to_login()
    {
        using var client = _webApplicationFactory.CreateClient(false);
        using var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Given_valid_credentials_should_be_logged_in()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains("Logout", responseContent);
    }
}
