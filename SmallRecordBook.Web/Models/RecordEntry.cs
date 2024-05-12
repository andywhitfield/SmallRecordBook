using System.ComponentModel.DataAnnotations;

namespace SmallRecordBook.Web.Models;

public class RecordEntry
{
    public int RecordEntryId { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public required UserAccount UserAccount { get; set; }
    [Required]
    public required DateOnly EntryDate { get; set; }
    [Required]
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReminderDate { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}