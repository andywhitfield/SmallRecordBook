using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public interface IRecordRepository
{
    Task<RecordEntry> AddAsync(UserAccount user, DateOnly entryDate, string title, string? description, DateOnly? reminderDate);
}