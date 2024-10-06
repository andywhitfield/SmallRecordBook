namespace SmallRecordBook.Web.Controllers.ApiModels;

public record RecordEntryApiModel(int RecordEntryId, DateOnly EntryDate, string EntryDateDesc,
    string Title, string? Description, IEnumerable<string> Tags, DateOnly? ReminderDate,
    string? ReminderDateDesc, string ItemCss);
