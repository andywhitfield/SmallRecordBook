using System.Security.Claims;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public interface IUserAccountRepository
{
    Task<UserAccount> CreateNewUserAsync(string email, byte[] credentialId, byte[] publicKey, byte[] userHandle);
    Task<UserAccount?> GetAsync(int userAccountId);
    Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user);
    Task<UserAccount?> GetUserAccountOrNullAsync(ClaimsPrincipal user);
    Task<UserAccount?> GetUserAccountByEmailAsync(string email);
    IAsyncEnumerable<UserAccountCredential> GetUserAccountCredentialsAsync(UserAccount user);
    Task<UserAccountCredential?> GetUserAccountCredentialsByUserHandleAsync(byte[] userHandle);
    Task SetSignatureCountAsync(UserAccountCredential userAccountCredential, uint signatureCount);
}