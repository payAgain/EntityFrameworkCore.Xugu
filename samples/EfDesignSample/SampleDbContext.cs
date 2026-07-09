using Microsoft.EntityFrameworkCore;

namespace EfDesignSample;

public class Blog
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
}

public class SampleDbContext : DbContext
{
    public SampleDbContext(DbContextOptions<SampleDbContext> options)
        : base(options)
    {
    }

    public DbSet<Blog> Blogs => Set<Blog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.UseIdentityColumns();
}
