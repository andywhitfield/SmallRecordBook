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

    [TestMethod]
    public async Task Should_show_all_items()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Currency = "£",
                Amount = 10.99m,
                Description = "Item1 in pounds"
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Currency = "$",
                Amount = 1200,
                Description = "Item2 in dollars, with no decimal places"
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser,
                Currency = "£",
                Amount = 5.1m
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 with no amount"
            });
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        Assert.Contains(">&#xA3;10.99<", responseContent);
        Assert.Contains(">Item1 in pounds<", responseContent);
        Assert.Contains(">$1,200<", responseContent);
        Assert.Contains(">Item2 in dollars, with no decimal places<", responseContent);
        Assert.Contains(">&#xA3;5.10<", responseContent);
        Assert.Contains(">Item4 with no amount<", responseContent);
        Assert.DoesNotContain("Total amount:", responseContent, message: "Total amount should not be shown when listing all items");
    }

    [TestMethod]
    public async Task Should_only_show_linked_items()
    {
        var linkGuid1 = Guid.NewGuid();
        var linkGuid2 = Guid.NewGuid();
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntriesAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 linked with Item4",
                LinkReference = linkGuid1,
                Currency = "£",
                Amount = 1099.99m
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 linked with Item3",
                LinkReference = linkGuid2
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser,
                Description = "Item3 linked with Item2",
                LinkReference = linkGuid2
            },
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 linked with Item1",
                LinkReference = linkGuid1,
                Currency = "$",
                Amount = 2100m
            });
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/?link={linkGuid1}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item1 linked with Item4"));
        StringAssert.Matches(responseContent, new("Item4 linked with Item1"));
        StringAssert.DoesNotMatch(responseContent, new("Item2 linked with Item3"));
        StringAssert.DoesNotMatch(responseContent, new("Item3 linked with Item2"));
        Assert.Contains(">Amount total: $2,100; &#xA3;1,099.99<", responseContent);
    }

    [TestMethod]
    public async Task Should_only_show_items_with_tag()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 with tag1",
                Currency = "$",
                Amount = 100m
            },
            "tag1");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 with tag2"
            },
            "tag2");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser,
                Description = "Item3 with tag1 and tag2",
                Currency = "$",
                Amount = 2100m
            },
            "tag1", "tag2");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 with tag3"
            },
            "tag3");
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/?tag=tag1");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item1 with tag1"));
        StringAssert.Matches(responseContent, new("Item3 with tag1 and tag2"));
        StringAssert.DoesNotMatch(responseContent, new("Item2 with tag2"));
        StringAssert.DoesNotMatch(responseContent, new("Item4 with tag3"));
        Assert.Contains(">Amount total: $2,200<", responseContent);
    }

    [TestMethod]
    public async Task Should_only_show_items_matching_search()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser,
                Description = "Item1 matching 'Find-Value' in the description",
                Currency = "$",
                Amount = 1110m
            },
            "tag");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser,
                Description = "Item2 not matching anything"
            },
            "tag");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3 find-values title",
                UserAccount = testUser,
                Description = "Item3 matching title",
                Currency = "£",
                Amount = 100.99m
            },
            "tag", "other-tag");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item4",
                UserAccount = testUser,
                Description = "Item4 not matching anything"
            },
            "other-tag");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item5",
                UserAccount = testUser,
                Description = "Item5 matching 'find-value' in a tag",
                Currency = "$",
                Amount = 2100m
            },
            "tag", "a-find-value-tag");
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/?find=find-value");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new("Item1 matching &#x27;Find-Value&#x27; in the description"));
        StringAssert.Matches(responseContent, new("Item3 find-values title"));
        StringAssert.Matches(responseContent, new("Item5 matching &#x27;find-value&#x27; in a tag"));
        StringAssert.DoesNotMatch(responseContent, new("Item2 not matching anything"));
        StringAssert.DoesNotMatch(responseContent, new("Item4 not matching anything"));
        Assert.Contains(">Amount total: $3,210; &#xA3;100.99<", responseContent);
    }

    [TestMethod]
    public async Task Should_display_tags()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser
            }, "tag1", "tag+2", "tag>3");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser
            }, "tag'4", "pound£");
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync($"/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();

        Assert.Contains("Item1", responseContent);
        Assert.Contains("tag=tag1", responseContent);
        Assert.Contains(">tag1<", responseContent);
        Assert.Contains("tag=tag%2B2", responseContent);
        Assert.Contains(">tag&#x2B;2<", responseContent);
        Assert.Contains("tag=tag%3E3", responseContent);
        Assert.Contains(">tag&gt;3<", responseContent);

        Assert.Contains("Item2", responseContent);
        Assert.Contains("tag=tag%274", responseContent);
        Assert.Contains(">tag&#x27;4<", responseContent);
        Assert.Contains("tag=pound%C2%A3", responseContent);
        Assert.Contains(">pound&#xA3;<", responseContent);
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
