using System.ComponentModel.DataAnnotations;

namespace SmallRecordBook.Web.Models;

public class UserAccount
{
    public int UserAccountId { get; set; }
    [Required]
    public string Email { get; set; } = "";
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}