using System.ComponentModel.DataAnnotations;

namespace SmallRecordBook.Web.Models;

public class RecordEntryTag
{
    public int RecordEntryTagId { get; set; }
    public int RecordEntryId { get; set; }
    [Required]
    public required RecordEntry RecordEntry { get; set; }
    [Required]
    public required string Tag { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}