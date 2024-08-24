using System.Net;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class HomeTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Given_no_credentials_should_redirect_to_login()
    {
        using var client = _webApplicationFactory.CreateClient(false);
        using var response = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task Given_valid_credentials_should_be_logged_in()
    {
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, "Logout");
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
