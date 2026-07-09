using Microsoft.EntityFrameworkCore;

namespace IntegrationSample.MinimalApi.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("INT_SAMPLE_ITEMS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseXuguIdentityColumn();
            entity.Property(e => e.Name).HasMaxLength(128).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSDATE");
        });
    }
}

public class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
