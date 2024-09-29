using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmallRecordBook.Web.Controllers.ApiModels;
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
    [HttpGet("recordentries")]
    public async Task<IEnumerable<RecordEntryApiModel>> GetRecordEntries([FromQuery] DateTime date)
    {
        DateOnly dateMonth = new(date.Year, date.Month, 1);
        logger.LogInformation("Getting record entries for {Date} and the previous/next months", date);
        var user = await userAccountRepository.GetUserAccountAsync(User);
        return recordRepository
            .GetBy(user, re => re.EntryDate >= dateMonth.AddMonths(-1) && re.EntryDate < dateMonth.AddMonths(2))
            .Select(re => new RecordEntryApiModel(
                re.RecordEntryId, re.EntryDate, re.EntryDate.ToString("dddd dd MMMM yyyy"),
                re.Title, re.Description, re.ActiveRecordEntryTags.Select(t => t.Tag),
                re.ReminderDate, re.ListItemCss()));
    }
}