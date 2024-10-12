using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class RecordRepository(
    ILogger<RecordRepository> logger,
    SqliteDataContext context)
    : IRecordRepository
{
    public async ValueTask<RecordEntry?> GetByIdAsync(UserAccount user, int recordEntryId)
    {
        var recordEntry = await context.RecordEntries.FindAsync(recordEntryId);
        return recordEntry == null || recordEntry.DeletedDateTime != null || recordEntry.UserAccountId != user.UserAccountId ? null : recordEntry;
    }

    public IEnumerable<RecordEntry> GetAll(UserAccount user)
        => context.RecordEntries
            .Where(e => e.UserAccountId == user.UserAccountId && e.DeletedDateTime == null)
            .OrderByDescending(e => e.EntryDate)
            .ThenBy(e => e.Title);

    public IQueryable<RecordEntry> GetBy(UserAccount user, Expression<Func<RecordEntry, bool>> condition)
        => context.RecordEntries
            .Where(e => e.UserAccountId == user.UserAccountId && e.DeletedDateTime == null)
            .Where(condition);

    public async Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate,
        string? tags, int? parentRecordEntryId)
    {
        RecordEntry newRecordEntry = new()
        {
            UserAccount = user,
            EntryDate = entryDate,
            Title = title,
            Description = description,
            ReminderDate = reminderDate,
        };

        // if there's a parent, we need to get the link guid from the parent entry or create a new one
        if (parentRecordEntryId != null)
        {
            var parentRecordEntry = await GetByIdAsync(user, parentRecordEntryId.Value);
            if (parentRecordEntry == null || parentRecordEntry.UserAccountId != user.UserAccountId)
                throw new ArgumentException("UserAccount and parent RecordEntry user mismatch", nameof(parentRecordEntryId));
            
            if (parentRecordEntry.LinkReference == null)
            {
                logger.LogInformation("Parent record entry {ParentRecordEntryId} has no link reference, creating a new one", parentRecordEntry.RecordEntryId);
                parentRecordEntry.LinkReference = Guid.NewGuid();
                parentRecordEntry.LastUpdateDateTime = newRecordEntry.CreatedDateTime;
            }

            logger.LogInformation("Assiging parent record entry {ParentRecordEntryId} with link reference {ParentRecordEntryLinkReference} to the new entry",
                parentRecordEntry.RecordEntryId, parentRecordEntry.LinkReference);
            newRecordEntry.LinkReference = parentRecordEntry.LinkReference;
        }

        context.RecordEntries.Add(newRecordEntry);

        var recordEntryTags = (tags ?? "")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet()
            .Select(t => new RecordEntryTag
            {
                Tag = t,
                RecordEntry = newRecordEntry,
                CreatedDateTime = newRecordEntry.CreatedDateTime
            });
        context.RecordEntryTags.AddRange(recordEntryTags);

        logger.LogDebug("Saving new record entry for {UserAccountId}: [{Title}]", newRecordEntry.UserAccount.UserAccountId, newRecordEntry.Title);
        await context.SaveChangesAsync();

        newRecordEntry.RecordEntryTags = recordEntryTags;
        return newRecordEntry;
    }

    public IEnumerable<(string Tag, int TagCount)> GetTags(UserAccount user) =>
        context.RecordEntryTags
            .Include(ret => ret.RecordEntry)
            .Where(ret => ret.DeletedDateTime == null && ret.RecordEntry.UserAccountId == user.UserAccountId && ret.RecordEntry.DeletedDateTime == null)
            .Select(ret => ret.Tag)
            .GroupBy(t => t)
            .OrderByDescending(t => t.Count())
            .ThenBy(t => t.Key)
            .Select(t => ValueTuple.Create(t.Key, t.Count()));

    public async Task SaveAsync(UserAccount user, RecordEntry recordEntry, string? tags)
    {
        if (recordEntry.UserAccountId != user.UserAccountId)
            throw new ArgumentException("UserAccount and RecordEntry user mismatch", nameof(user));

        var updateDate = DateTime.UtcNow;
        recordEntry.LastUpdateDateTime = updateDate;

        // get existing tags, and remove any no longer needed; add new ones
        var existingTags = context.RecordEntryTags.Where(ret => ret.DeletedDateTime == null && ret.RecordEntryId == recordEntry.RecordEntryId);
        var newTags = (tags ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet();
        var tagsToDelete = existingTags.Where(t => !newTags.Contains(t.Tag));
        var tagsToAdd = newTags.Where(t => !existingTags.Any(et => et.Tag == t));

        context.RecordEntryTags.AddRange(tagsToAdd.Select(t => new RecordEntryTag { Tag = t, RecordEntry = recordEntry, CreatedDateTime = updateDate }));
        await tagsToDelete.ForEachAsync(t => t.DeletedDateTime = updateDate);

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(UserAccount user, RecordEntry recordEntry)
    {
        if (recordEntry.UserAccountId != user.UserAccountId)
            throw new ArgumentException("UserAccount and RecordEntry user mismatch", nameof(user));

        var deleteDate = DateTime.UtcNow;
        recordEntry.DeletedDateTime = deleteDate;
        await context.RecordEntryTags
            .Where(ret => ret.DeletedDateTime == null && ret.RecordEntryId == recordEntry.RecordEntryId)
            .ForEachAsync(t => t.DeletedDateTime = deleteDate);

        await context.SaveChangesAsync();
    }
}