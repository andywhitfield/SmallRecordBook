using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class RecordRepository(
    ILogger<RecordRepository> logger,
    SqliteDataContext context)
    : IRecordRepository
{
    public async Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate)
    {
        RecordEntry newRecordEntry = new() {
            UserAccount = user,
            EntryDate = entryDate,
            Title = title,
            Description = description,
            ReminderDate = reminderDate
        };
        context.RecordEntries.Add(newRecordEntry);

        logger.LogDebug("Saving new record entry for {UserAccountId}: [{Title}]", newRecordEntry.UserAccount.UserAccountId, newRecordEntry.Title);
        await context.SaveChangesAsync();
        return newRecordEntry;
    }
}