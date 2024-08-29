using System.Linq.Expressions;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public interface IRecordRepository
{
    ValueTask<RecordEntry?> GetByIdAsync(UserAccount user, int recordEntryId);
    IEnumerable<RecordEntry> GetAll(UserAccount user);
    IEnumerable<RecordEntry> GetBy(UserAccount user, Expression<Func<RecordEntry, bool>> condition);
    Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate, string? tags, int? parentRecordEntryId);
    Task SaveAsync(UserAccount user, RecordEntry recordEntry, string? tags);
    Task DeleteAsync(UserAccount user, RecordEntry recordEntry);
    IEnumerable<(string Tag, int TagCount)> GetTags(UserAccount user);
}