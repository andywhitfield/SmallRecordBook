using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Repositories;
using SmallRecordBook.Web.Services;

namespace SmallRecordBook.Web.Controllers;

[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Xml)]
public class FeedController(ILogger<FeedController> logger,
    IUserAccountRepository userAccountRepository,
    IUserFeedRepository userFeedRepository,
    IRecordRepository recordRepository,
    IFeedGenerator feedGenerator)
    : ControllerBase
{
    [HttpGet("{feedIdentifier}")]
    public async Task<IActionResult> Index([FromRoute, Required] string feedIdentifier)
    {
        var userFeed = await userFeedRepository.FindByIdentifierAsync(feedIdentifier);
        if (userFeed == null)
        {
            logger.LogInformation("No user feed found with identifier: {feedIdentifier}", feedIdentifier);
            return NotFound();
        }

        var user = await userAccountRepository.GetAsync(userFeed.UserAccountId);
        if (user == null)
        {
            logger.LogInformation("No user account found, associated with feed identifier: {feedIdentifier}", feedIdentifier);
            return NotFound();
        }

        var upcomingReminders = await recordRepository
            .GetBy(user,
                re => re.ReminderDate != null &&
                (re.ReminderDone == null || !re.ReminderDone.Value) &&
                re.ReminderDate <= DateOnly.FromDateTime(DateTime.Today.AddMonths(1)))
            .ToListAsync();

        var itemHash = GenerateHash(upcomingReminders);
        if (userFeed.ItemHash != itemHash)
        {
            userFeed.ItemHash = itemHash;
            await userFeedRepository.SaveAsync(userFeed);
        }
        
        var (content, contentType, encoding) = feedGenerator.GenerateFeed($"{Request.Scheme}://{Request.Host}", upcomingReminders, userFeed);
        return Content(content, contentType, encoding);
    }

    private static int GenerateHash(IEnumerable<RecordEntry> recordEntries)
    {
        HashCode hash = new();

        if (recordEntries.Any())
            hash.Add(DateTime.Today);

        foreach (var recordEntry in recordEntries)
            hash.Add(recordEntry.LastUpdateDateTime ?? recordEntry.CreatedDateTime);

        return hash.ToHashCode();
    }
}