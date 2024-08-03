using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public interface IRecordRepository
{
    IEnumerable<RecordEntry> GetAll(UserAccount user);
    Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate, string? tags);
    IEnumerable<string> GetTags(UserAccount user);
}