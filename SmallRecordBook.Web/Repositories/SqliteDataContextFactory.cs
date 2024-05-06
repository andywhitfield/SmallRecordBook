
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SmallRecordBook.Web.Repositories;

// used by the migrations tool only
public class SqliteDataContextFactory : IDesignTimeDbContextFactory<SqliteDataContext>
{
    public SqliteDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqliteDataContext>();
        optionsBuilder.UseSqlite("Data Source=SmallRecordBook.Web/smallrecordbook.db");
        return new SqliteDataContext(optionsBuilder.Options);
    }
}
