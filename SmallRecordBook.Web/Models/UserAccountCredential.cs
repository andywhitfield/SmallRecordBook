using System.ComponentModel.DataAnnotations;

namespace SmallRecordBook.Web.Models;

public class UserAccountCredential
{
    public int UserAccountCredentialId { get; set; }
    public int UserAccountId { get; set; }
    [Required]
    public required UserAccount UserAccount { get; set; }

    public byte[] CredentialId { get; set; } = [];
    public byte[] PublicKey { get; set; } = [];
    public byte[] UserHandle { get; set; } = [];
    public uint SignatureCount { get; set; } = 0;

    public DateTime CreatedDateTime { get; set; } = DateTime.UtcNow;
    public DateTime? LastUpdateDateTime { get; set; }
    public DateTime? DeletedDateTime { get; set; }
}