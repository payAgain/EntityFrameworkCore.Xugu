using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;

/// <summary>
/// Phase 10.101 — MonsterFixup subset (change-tracking fixup on Xugu-compatible models).
/// </summary>
[Collection("XuguMonsterFixup")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class MonsterFixupXuguTests(MonsterFixupFixture fixture)
{
    [SkippableFact]
    public void Setting_foreign_key_fixes_up_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        using var context = fixture.CreateContext();
        var product = context.Products.Single(p => p.Id == 1);
        var other = context.Categories.Single(c => c.Id == 2);

        product.CategoryId = 2;
        context.ChangeTracker.DetectChanges();

        Assert.Same(other, product.Category);
    }

    [SkippableFact]
    public void Setting_navigation_fixes_up_foreign_key()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategories();

        using var context = fixture.CreateContext();
        var product = new Product { Name = "New" };
        var category = context.Categories.Single(c => c.Id == 1);

        product.Category = category;
        context.Products.Add(product);

        Assert.Equal(1, product.CategoryId);
        Assert.Contains(product, category.Products);
    }

    [SkippableFact]
    public void Adding_to_collection_fixes_up_inverse_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategories();

        using var context = fixture.CreateContext();
        var category = context.Categories.Include(c => c.Products).Single(c => c.Id == 1);
        var product = new Product { Name = "Added" };

        category.Products.Add(product);
        context.ChangeTracker.DetectChanges();

        Assert.Same(category, product.Category);
        Assert.Equal(1, product.CategoryId);
    }

    [SkippableFact]
    public void Removing_from_collection_clears_inverse_navigation()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        using var context = fixture.CreateContext();
        var category = context.Categories.Include(c => c.Products).Single(c => c.Id == 1);
        var product = category.Products.Single();

        category.Products.Remove(product);
        context.ChangeTracker.DetectChanges();

        Assert.DoesNotContain(product, category.Products);
        Assert.Null(product.Category);
    }

    [SkippableFact]
    public void Changing_foreign_key_updates_both_collections()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedTwoCategoriesWithProduct();

        using var context = fixture.CreateContext();
        var product = context.Products.Include(p => p.Category).Single(p => p.Id == 1);
        var oldCategory = product.Category!;
        var newCategory = context.Categories.Include(c => c.Products).Single(c => c.Id == 2);

        product.CategoryId = 2;
        context.ChangeTracker.DetectChanges();

        Assert.DoesNotContain(product, oldCategory.Products);
        Assert.Contains(product, newCategory.Products);
        Assert.Same(newCategory, product.Category);
    }

    [SkippableFact]
    public void One_to_one_setting_dependent_fixes_up_principal()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedProducts();

        using var context = fixture.CreateContext();
        var product = context.Products.Single(p => p.Id == 1);
        var detail = new ProductDetail { Notes = "Detail" };

        product.Detail = detail;
        context.ChangeTracker.DetectChanges();

        Assert.Same(product, detail.Product);
        Assert.Equal(1, detail.ProductId);
    }

    [SkippableFact]
    public void One_to_one_setting_principal_fixes_up_dependent()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedProducts();

        using var context = fixture.CreateContext();
        var product = context.Products.Single(p => p.Id == 1);
        var detail = new ProductDetail { ProductId = 1, Notes = "Detail" };

        detail.Product = product;
        context.ProductDetails.Add(detail);

        Assert.Same(detail, product.Detail);
    }

    [SkippableFact]
    public void DetectChanges_fixes_up_after_manual_fk_assignment()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        using var context = fixture.CreateContext();
        var product = context.Products.Single(p => p.Id == 1);
        var other = context.Categories.Single(c => c.Id == 2);

        product.CategoryId = 2;
        context.ChangeTracker.DetectChanges();

        Assert.Same(other, product.Category);
        Assert.Equal(EntityState.Modified, context.Entry(product).State);
    }

    [SkippableFact]
    public async Task Attach_graph_fixes_up_relationships()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        await using var context = fixture.CreateContext();
        var category = new Category { Id = 1, Name = "Cat" };
        var product = new Product { Id = 1, Name = "Prod", CategoryId = 1 };

        context.Attach(category);
        context.Attach(product);
        context.Entry(product).Reference(p => p.Category).CurrentValue = category;

        Assert.Same(category, product.Category);
        Assert.Contains(product, category.Products);
    }

    [SkippableTheory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Optional_foreign_key_can_be_null(int categoryId)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategories();

        using var context = fixture.CreateContext();
        var tag = new Tag { Name = $"Tag-{categoryId}", CategoryId = categoryId == 3 ? null : categoryId };
        context.Tags.Add(tag);

        if (categoryId != 3)
        {
            Assert.Same(context.Categories.Single(c => c.Id == categoryId), tag.Category);
        }
        else
        {
            Assert.Null(tag.Category);
        }
    }

    [SkippableFact]
    public void Self_reference_parent_fixes_up_children()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var root = new Node { Id = 1, Name = "Root" };
        var child = new Node { Id = 2, Name = "Child", ParentId = 1 };

        context.Nodes.AddRange(root, child);
        child.Parent = root;

        Assert.Contains(child, root.Children);
        Assert.Same(root, child.Parent);
    }

    [SkippableFact]
    public void Many_to_many_add_fixes_up_both_navigations()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedAuthorsAndBooks();

        using var context = fixture.CreateContext();
        var author = context.Authors.Include(a => a.Books).Single();
        var book = context.Books.Single(b => b.Id == 2);

        author.Books.Add(book);
        context.ChangeTracker.DetectChanges();

        Assert.Contains(author, book.Authors);
    }

    [SkippableFact]
    public void Many_to_many_remove_fixes_up_both_navigations()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedAuthorsAndBooksWithLink();

        using var context = fixture.CreateContext();
        var author = context.Authors.Include(a => a.Books).Single();
        var book = author.Books.Single();

        author.Books.Remove(book);
        context.ChangeTracker.DetectChanges();

        Assert.DoesNotContain(author, book.Authors);
    }

    [SkippableFact]
    public void Store_generated_temporary_key_cleared_after_save()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var category = new Category { Name = "Temp" };
        var entry = context.Categories.Add(category);

        Assert.True(entry.Property(e => e.Id).IsTemporary);

        context.SaveChanges();

        Assert.False(entry.Property(e => e.Id).IsTemporary);
        Assert.True(category.Id > 0);
    }

    [SkippableFact]
    public void Temp_value_can_be_made_permanent_before_save()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var category = new Category { Name = "Permanent" };
        var entry = context.Categories.Add(category);
        var tempValue = entry.Property(e => e.Id).CurrentValue;

        entry.Property(e => e.Id).IsTemporary = false;
        context.SaveChanges();

        Assert.False(entry.Property(e => e.Id).IsTemporary);
        Assert.Equal(tempValue, entry.Property(e => e.Id).CurrentValue);
    }

    [SkippableTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Fixup_persists_after_save(bool changeCategory)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        await using (var context = fixture.CreateContext())
        {
            var product = context.Products.Single(p => p.Id == 1);
            if (changeCategory)
            {
                product.CategoryId = 2;
            }

            await context.SaveChangesAsync();
        }

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Products.SingleAsync(p => p.Id == 1);
        Assert.Equal(changeCategory ? 2 : 1, loaded.CategoryId);
    }

    [SkippableFact]
    public void Adding_principal_and_dependents_in_one_graph_fixes_up()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        using var context = fixture.CreateContext();
        var category = new Category { Name = "Bundle" };
        var product = new Product { Name = "Item" };
        category.Products.Add(product);
        context.Categories.Add(category);

        Assert.Same(category, product.Category);
        Assert.Equal(0, product.CategoryId);
    }

    [SkippableFact]
    public void Delete_principal_cascades_and_clears_tracking()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        using var context = fixture.CreateContext();
        var category = context.Categories.Include(c => c.Products).Single(c => c.Id == 1);
        context.Categories.Remove(category);
        context.SaveChanges();

        Assert.Equal(0, context.Products.Count());
        Assert.DoesNotContain(context.Categories.ToList(), c => c.Id == 1);
    }

    [SkippableFact]
    public void Navigation_fixes_up_when_principal_key_changes()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategoryWithProducts();

        using var context = fixture.CreateContext();
        var product = context.Products.Include(p => p.Category).Single(p => p.Id == 1);
        var replacement = context.Categories.Single(c => c.Id == 2);

        product.Category = replacement;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(2, product.CategoryId);
    }

    [SkippableTheory]
    [InlineData("alpha")]
    [InlineData("beta")]
    [InlineData("gamma")]
    public async Task Store_generated_identity_populates_after_save(string name)
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int id;
        await using (var context = fixture.CreateContext())
        {
            var category = new MonsterFixupXuguTests.Category { Name = name };
            context.Categories.Add(category);
            await context.SaveChangesAsync();
            id = category.Id;
            Assert.True(id > 0);
        }

        await using var verify = fixture.CreateContext();
        Assert.Equal(name, (await verify.Categories.FindAsync(id))!.Name);
    }

    public sealed class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = [];
    }

    public sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ProductDetail? Detail { get; set; }
    }

    public sealed class ProductDetail
    {
        public int ProductId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public Product Product { get; set; } = null!;
    }

    public sealed class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
    }

    public sealed class Node
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public Node? Parent { get; set; }
        public List<Node> Children { get; set; } = [];
    }

    public sealed class Author
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Book> Books { get; set; } = [];
    }

    public sealed class Book
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<Author> Authors { get; set; } = [];
    }

    public sealed class MonsterFixupContext : DbContext
    {
        private readonly XuguTestStore _store;

        public MonsterFixupContext(DbContextOptions<MonsterFixupContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductDetail> ProductDetails => Set<ProductDetail>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<Node> Nodes => Set<Node>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Book> Books => Set<Book>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var category = _store.FormatTableName("MF_CATEGORY");
            var product = _store.FormatTableName("MF_PRODUCT");
            var detail = _store.FormatTableName("MF_DETAIL");
            var tag = _store.FormatTableName("MF_TAG");
            var node = _store.FormatTableName("MF_NODE");
            var author = _store.FormatTableName("MF_AUTHOR");
            var book = _store.FormatTableName("MF_BOOK");
            var bookAuthor = _store.FormatTableName("MF_BOOK_AUTHOR");

            modelBuilder.Entity<Category>(e =>
            {
                e.ToTable(category);
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
                e.HasMany(x => x.Products).WithOne(x => x.Category).HasForeignKey(x => x.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.ToTable(product);
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
                e.Property(x => x.CategoryId).HasColumnName("CATEGORY_ID");
                e.HasOne(x => x.Detail).WithOne(x => x.Product).HasForeignKey<ProductDetail>(x => x.ProductId);
            });

            modelBuilder.Entity<ProductDetail>(e =>
            {
                e.ToTable(detail);
                e.HasKey(x => x.ProductId);
                e.Property(x => x.ProductId).HasColumnName("PRODUCT_ID");
                e.Property(x => x.Notes).HasColumnName("NOTES").HasMaxLength(500);
            });

            modelBuilder.Entity<Tag>(e =>
            {
                e.ToTable(tag);
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
                e.Property(x => x.CategoryId).HasColumnName("CATEGORY_ID");
                e.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId).IsRequired(false);
            });

            modelBuilder.Entity<Node>(e =>
            {
                e.ToTable(node);
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID");
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
                e.Property(x => x.ParentId).HasColumnName("PARENT_ID");
                e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId)
                    .IsRequired(false);
            });

            modelBuilder.Entity<Author>(e =>
            {
                e.ToTable(author);
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
                e.HasMany(x => x.Books).WithMany(x => x.Authors)
                    .UsingEntity(j =>
                    {
                        j.ToTable(bookAuthor);
                        j.IndexerProperty<int>("AuthorsId").HasColumnName("AUTHORS_ID");
                        j.IndexerProperty<int>("BooksId").HasColumnName("BOOKS_ID");
                    });
            });

            modelBuilder.Entity<Book>(e =>
            {
                e.ToTable(book);
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).HasColumnName("ID").ValueGeneratedOnAdd();
                e.Property(x => x.Title).HasColumnName("TITLE").HasMaxLength(200);
            });
        }
    }
}
