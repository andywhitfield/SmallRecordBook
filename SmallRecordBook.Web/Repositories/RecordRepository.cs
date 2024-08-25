using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class RecordRepository(
    ILogger<RecordRepository> logger,
    SqliteDataContext context)
    : IRecordRepository
{
    public ValueTask<RecordEntry?> GetByIdAsync(int recordEntryId) => context.RecordEntries.FindAsync(recordEntryId);

    public IEnumerable<RecordEntry> GetAll(UserAccount user)
        => context.RecordEntries
            .Where(e => e.UserAccountId == user.UserAccountId && e.DeletedDateTime == null)
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.Title);

    public async Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate, string? tags)
    {
        RecordEntry newRecordEntry = new()
        {
            UserAccount = user,
            EntryDate = entryDate,
            Title = title,
            Description = description,
            ReminderDate = reminderDate,
        };
        context.RecordEntries.Add(newRecordEntry);

        var recordEntryTags = (tags ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(t => new RecordEntryTag
            {
                Tag = t,
                RecordEntry = newRecordEntry
            });
        context.RecordEntryTags.AddRange(recordEntryTags);

        logger.LogDebug("Saving new record entry for {UserAccountId}: [{Title}]", newRecordEntry.UserAccount.UserAccountId, newRecordEntry.Title);
        await context.SaveChangesAsync();

        newRecordEntry.RecordEntryTags = recordEntryTags;
        return newRecordEntry;
    }

    public IEnumerable<string> GetTags(UserAccount user) =>
        context.RecordEntryTags
            .Include(ret => ret.RecordEntry)
            .Where(ret => ret.DeletedDateTime == null && ret.RecordEntry.UserAccountId == user.UserAccountId && ret.RecordEntry.DeletedDateTime == null)
            .Select(ret => ret.Tag)
            .Distinct()
            .OrderBy(t => t);
}