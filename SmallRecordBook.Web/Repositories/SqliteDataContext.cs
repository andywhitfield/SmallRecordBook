using Microsoft.EntityFrameworkCore;
using SmallRecordBook.Web.Models;

namespace SmallRecordBook.Web.Repositories;

public class SqliteDataContext(DbContextOptions<SqliteDataContext> options) : DbContext(options), ISqliteDataContext
{
    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<UserAccountCredential> UserAccountCredentials { get; set; }
    public void Migrate() => Database.Migrate();
}