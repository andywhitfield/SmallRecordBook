using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace SmallRecordBook.Web.Tests;

public class HomeTests(WebApplicationFactory<Program> webApplicationFactory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Should_display_expected_items()
    {
        using var client = webApplicationFactory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        /*
        responseContent.Should().Contain("Logout")
            .And.Contain("1 overdue and 1 due today (2)")
            .And.Contain("All (3)")
            .And.Contain("All upcoming (2)")
            .And.Contain("Test list (1)")
            .And.Contain("Test item 1")
            .And.Contain("Test item 2")
            .And.Contain("Test item 3")
            .And.Contain("Due yesterday")
            .And.Contain("Repeats every day")
            .And.Contain("Due today");
            */
    }
}