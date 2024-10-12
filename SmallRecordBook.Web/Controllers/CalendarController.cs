using System.Collections.Immutable;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Controllers.ApiModels;
using SmallRecordBook.Web.Models;
using SmallRecordBook.Web.Pages.ViewModels;
using SmallRecordBook.Web.Repositories;

namespace SmallRecordBook.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public class CalendarController(ILogger<CalendarController> logger, IUserAccountRepository userAccountRepository,
    IRecordRepository recordRepository)
    : ControllerBase
{
    private static IQueryable<RecordEntry> WhereLinkTagFindFilter(IQueryable<RecordEntry> query, Guid? link, string? tag, string? find)
    {
        if (link != null)
            return query.Where(re => re.LinkReference == link);
        if (!string.IsNullOrEmpty(tag))
            return query.Where(re => re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && ret.Tag == tag));
        if (!string.IsNullOrEmpty(find))
        {
            var like = $"%{find.Trim()}%";
            return query.Where(re =>
                (re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && EF.Functions.Like(ret.Tag, like))) ||
                EF.Functions.Like(re.Title, like) ||
                (re.Description != null && EF.Functions.Like(re.Description, like)));
        }

        return query;
    }

    [HttpGet("recordentries")]
    public async IAsyncEnumerable<RecordEntryApiModel> GetRecordEntries([FromQuery] DateTime date,
        [FromQuery] Guid? link, [FromQuery] string? tag, [FromQuery] string? find)
    {
        DateOnly dateMonth = new(date.Year, date.Month, 1);
        logger.LogInformation("Getting record entries for {Date} and the previous/next months", date);
        var user = await userAccountRepository.GetUserAccountAsync(User);
        IQueryable<RecordEntry> query = recordRepository.GetBy(user, re => re.EntryDate >= dateMonth.AddMonths(-1) && re.EntryDate < dateMonth.AddMonths(2));
        if (link != null)
        {
            query = query.Where(re => re.LinkReference == link);
        }
        else if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(re => re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && ret.Tag == tag));
        }
        else if (!string.IsNullOrEmpty(find))
        {
            var like = $"%{find.Trim()}%";
            query = query.Where(re =>
                re.EntryDate >= dateMonth.AddMonths(-1) && re.EntryDate < dateMonth.AddMonths(2) && (
                    (re.RecordEntryTags != null && re.RecordEntryTags.Any(ret => ret.DeletedDateTime == null && EF.Functions.Like(ret.Tag, like))) ||
                    EF.Functions.Like(re.Title, like) ||
                    (re.Description != null && EF.Functions.Like(re.Description, like)))).ToImmutableList().AsQueryable();
        }

        foreach (var recordEntry in query)
        {
            yield return new RecordEntryApiModel(
                recordEntry.RecordEntryId, recordEntry.EntryDate, recordEntry.EntryDate.ToString("dddd dd MMMM yyyy"),
                recordEntry.Title, recordEntry.Description, recordEntry.ActiveRecordEntryTags.Select(t => t.Tag),
                recordEntry.ReminderDate, recordEntry.ReminderDate?.ToString("dddd dd MMMM yyyy"),
                recordEntry.ListItemCss());
        }
    }

    [HttpGet("recordentrycounts")]
    public async Task<RecordEntryApiCountsModel> GetRecordEntryCounts([FromQuery] DateTime date,
        [FromQuery] Guid? link, [FromQuery] string? tag, [FromQuery] string? find)
    {
        DateOnly dateMonth = new(date.Year, date.Month, 1);
        DateOnly nextDateMonth = dateMonth.AddMonths(1);
        logger.LogInformation("Getting record entry counts for {Date}", date);
        var user = await userAccountRepository.GetUserAccountAsync(User);
        return new(
            WhereLinkTagFindFilter(recordRepository.GetBy(user, re => re.EntryDate < dateMonth), link, tag, find).Count(),
            WhereLinkTagFindFilter(recordRepository.GetBy(user, re => re.EntryDate >= nextDateMonth), link, tag, find).Count());
    }
}