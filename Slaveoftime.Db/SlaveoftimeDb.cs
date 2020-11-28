namespace Slaveoftime.Db;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


public class SlaveoftimeDb : DbContext
{
    public SlaveoftimeDb(DbContextOptions<SlaveoftimeDb> options) : base(options) { }

    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
}

public class ApplicationContextDbFactory : IDesignTimeDbContextFactory<SlaveoftimeDb>
{
    SlaveoftimeDb IDesignTimeDbContextFactory<SlaveoftimeDb>.CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SlaveoftimeDb>();
        optionsBuilder.UseSqlite("Data Source=Slaveoftime.db");

        return new SlaveoftimeDb(optionsBuilder.Options);
    }
}
