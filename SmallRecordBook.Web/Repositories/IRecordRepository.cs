using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public interface IRecordRepository
{
    ValueTask<RecordEntry?> GetByIdAsync(UserAccount user, int recordEntryId);
    IEnumerable<RecordEntry> GetAll(UserAccount user);
    Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate, string? tags);
    Task SaveAsync(UserAccount user, RecordEntry recordEntry, string? tags);
    Task DeleteAsync(UserAccount user, RecordEntry recordEntry);
    IEnumerable<string> GetTags(UserAccount user);
}