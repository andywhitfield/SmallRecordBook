using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class SqliteDataContext(DbContextOptions<SqliteDataContext> options) : DbContext(options), ISqliteDataContext
{
    public DbSet<UserAccount> UserAccounts { get; set; } = null!;
    public DbSet<UserAccountCredential> UserAccountCredentials { get; set; } = null!;
    public DbSet<UserAccountSetting> UserAccountSettings { get; set; } = null!;
    public DbSet<RecordEntry> RecordEntries { get; set; } = null!;
    public DbSet<RecordEntryTag> RecordEntryTags { get; set; } = null!;
    public DbSet<UserFeed> UserFeeds { get; set; } = null!;
    public void Migrate() => Database.Migrate();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<RecordEntry>().Navigation(re => re.RecordEntryTags).AutoInclude();
}