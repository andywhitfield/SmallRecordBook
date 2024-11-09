using System.ComponentModel.DataAnnotations;

namespace SmallRecordBook.Web.Models;

public class UserFeed
{
    public int UserFeedId { get; set; }
    [Required]
    public required string UserFeedIdentifier { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public required UserAccount UserAccount { get; set; }
    public int ItemHash { get; set; }
    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}