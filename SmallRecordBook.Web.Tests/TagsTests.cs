using System.Net;

namespace SmallRecordBook.Web.Tests;

[TestClass]
public class TagsTests
{
    private readonly WebApplicationFactoryTest _webApplicationFactory = new();

    [TestInitialize]
    public Task InitializeAsync() => TestStubAuthHandler.AddTestUserAsync(_webApplicationFactory.Services);

    [TestMethod]
    public async Task Should_show_only_top_8_tags()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser
            },
            "tag-1", "tag-2", "tag-3");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser
            },
            "tag-3", "tag-4", "tag-5", "tag-6", "tag-7");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser
            },
            "tag-6", "tag-7", "tag-8", "tag-9");
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(responseContent, new(@"tag-1 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-2 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-3 \(2\)"));
        StringAssert.Matches(responseContent, new(@"tag-4 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-5 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-6 \(2\)"));
        StringAssert.Matches(responseContent, new(@"tag-7 \(2\)"));
        StringAssert.Matches(responseContent, new(@"tag-8 \(1\)"));
        StringAssert.DoesNotMatch(responseContent, new(@"tag-9 \(1\)"));
    }

    [TestMethod]
    public async Task Should_show_all_tags_on_tags_page()
    {
        var testUser = await TestStubAuthHandler.GetTestUserAsync(_webApplicationFactory.Services);
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item1",
                UserAccount = testUser
            },
            "tag-1", "tag-2", "tag-3");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item2",
                UserAccount = testUser
            },
            "tag-3", "tag-4", "tag-5", "tag-6", "tag-7");
        await _webApplicationFactory.AddRecordEntryAsync(
            new()
            {
                EntryDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Title = "Item3",
                UserAccount = testUser
            },
            "tag-6", "tag-7", "tag-8", "tag-9");
        using var client = _webApplicationFactory.CreateClient(true);
        using var response = await client.GetAsync("/tags");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-1"">tag-1</a></td><td>1</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-2"">tag-2</a></td><td>1</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-3"">tag-3</a></td><td>2</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-4"">tag-4</a></td><td>1</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-5"">tag-5</a></td><td>1</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-6"">tag-6</a></td><td>2</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-7"">tag-7</a></td><td>2</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-8"">tag-8</a></td><td>1</td></tr>");
        StringAssert.Contains(responseContent, @"<tr><td><a href=""/?tag=tag-9"">tag-9</a></td><td>1</td></tr>");

        // check the tags in the left-hand nav as well
        StringAssert.Matches(responseContent, new(@"tag-1 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-2 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-3 \(2\)"));
        StringAssert.Matches(responseContent, new(@"tag-4 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-5 \(1\)"));
        StringAssert.Matches(responseContent, new(@"tag-6 \(2\)"));
        StringAssert.Matches(responseContent, new(@"tag-7 \(2\)"));
        StringAssert.Matches(responseContent, new(@"tag-8 \(1\)"));
        StringAssert.DoesNotMatch(responseContent, new(@"tag-9 \(1\)"));
    }

    [TestCleanup]
    public void Cleanup() => _webApplicationFactory.Dispose();
}
