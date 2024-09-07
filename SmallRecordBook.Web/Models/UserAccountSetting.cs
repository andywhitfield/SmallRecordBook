using System.ComponentModel.DataAnnotations;

namespace SmallRecordBook.Web.Models;

public class UserAccountSetting
{
    public const string ViewListOrCalendar = nameof(ViewListOrCalendar);

    public int UserAccountSettingId { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public required UserAccount UserAccount { get; set; }    

    [Required]
    public required string SettingName { get; set; }
    [Required]
    public required string SettingValue { get; set; }

    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}
