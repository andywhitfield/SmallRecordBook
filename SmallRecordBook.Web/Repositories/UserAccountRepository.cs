using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class UserAccountRepository(ILogger<UserAccountRepository> logger, SqliteDataContext context)
    : IUserAccountRepository
{
    public Task<UserAccount?> GetAsync(int userAccountId) =>
        context.UserAccounts.SingleOrDefaultAsync(a => a.UserAccountId == userAccountId && a.DeletedDateTime == null);

    public async Task<UserAccount> CreateNewUserAsync(string email, byte[] credentialId, byte[] publicKey, byte[] userHandle)
    {
        var newUserAccount = context.UserAccounts.Add(new UserAccount { Email = email });
        context.UserAccountCredentials!.Add(new()
        {
            UserAccount = newUserAccount.Entity,
            CredentialId = credentialId,
            PublicKey = publicKey,
            UserHandle = userHandle
        });
        await context.SaveChangesAsync();
        return newUserAccount.Entity;
    }

    private string? GetEmailFromPrincipal(ClaimsPrincipal user)
    {
        logger.LogTrace("Getting email from user: {Name}: [{Claims}]", user?.Identity?.Name, string.Join(',', user?.Claims.Select(c => $"{c.Type}={c.Value}") ?? []));
        return user?.FindFirstValue(ClaimTypes.Name);
    }

    public async Task<UserAccount> GetUserAccountAsync(ClaimsPrincipal user) => (await GetUserAccountOrNullAsync(user)) ?? throw new ArgumentException($"No UserAccount for the user: {GetEmailFromPrincipal(user)}");

    public Task<UserAccount?> GetUserAccountOrNullAsync(ClaimsPrincipal user)
    {
        var email = GetEmailFromPrincipal(user);
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult((UserAccount?)null);

        return context.UserAccounts.FirstOrDefaultAsync(ua => ua.Email == email && ua.DeletedDateTime == null);
    }

    public Task<UserAccount?> GetUserAccountByEmailAsync(string email)
        => context.UserAccounts!.FirstOrDefaultAsync(a => a.DeletedDateTime == null && a.Email == email);

    public IAsyncEnumerable<UserAccountCredential> GetUserAccountCredentialsAsync(UserAccount user)
        => context.UserAccountCredentials!.Where(uac => uac.DeletedDateTime == null && uac.UserAccountId == user.UserAccountId).AsAsyncEnumerable();

    public Task<UserAccountCredential?> GetUserAccountCredentialsByUserHandleAsync(byte[] userHandle)
        => context.UserAccountCredentials!.FirstOrDefaultAsync(uac => uac.DeletedDateTime == null && uac.UserHandle.SequenceEqual(userHandle));

    public Task SetSignatureCountAsync(UserAccountCredential userAccountCredential, uint signatureCount)
    {
        userAccountCredential.SignatureCount = signatureCount;
        return context.SaveChangesAsync();
    }

    public async Task<string?> GetUserAccountSettingAsync(UserAccount userAccount, string settingName)
        => (await context.UserAccountSettings.FirstOrDefaultAsync(uas =>
                uas.UserAccountId == userAccount.UserAccountId &&
                uas.SettingName == settingName &&
                uas.DeletedDateTime == null
            ))?.SettingValue;

    public async Task<string> GetUserAccountSettingOrDefaultAsync(UserAccount userAccount, string settingName, string defaultValue)
        => await GetUserAccountSettingAsync(userAccount, settingName) ?? defaultValue;

    public async Task SetUserAccountSettingAsync(UserAccount userAccount, string settingName, string settingValue)
    {
        var existingSetting = await context.UserAccountSettings.FirstOrDefaultAsync(uas =>
            uas.UserAccountId == userAccount.UserAccountId &&
            uas.SettingName == settingName &&
            uas.DeletedDateTime == null
        );

        if (existingSetting == null)
        {
            context.UserAccountSettings.Add(new()
            {
                UserAccount = userAccount,
                SettingName = settingName,
                SettingValue = settingValue
            });
        }
        else
        {
            existingSetting.SettingValue = settingValue;
            existingSetting.LastUpdateDateTime = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task RemoveUserAccountSettingAsync(UserAccount userAccount, string settingName)
    {
        foreach (var setting in context.UserAccountSettings.Where(uas =>
            uas.UserAccountId == userAccount.UserAccountId &&
            uas.SettingName == settingName &&
            uas.DeletedDateTime == null))
        {
            setting.DeletedDateTime = DateTime.UtcNow;
        }
        
        await context.SaveChangesAsync();
    }
}
