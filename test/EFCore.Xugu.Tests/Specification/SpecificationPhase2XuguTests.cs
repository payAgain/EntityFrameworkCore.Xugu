using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;

/// <summary>
/// Phase 11.402 — Specification Tests Phase 2 subset.
/// </summary>
[Collection("XuguMonsterFixup")]
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
public class SpecificationPhase2XuguTests(MonsterFixupFixture fixture)
{
    [SkippableFact]
    public async Task One_to_many_insert_assigns_identity_to_both_ends()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedCategories();

        await using var context = fixture.CreateContext();
        var category = await context.Categories.SingleAsync(c => c.Id == 1);
        var product = new MonsterFixupXuguTests.Product { Name = "SpecP2" };
        category.Products.Add(product);
        await context.SaveChangesAsync();

        Assert.True(product.Id > 0);
        Assert.Equal(1, product.CategoryId);
    }

    [SkippableFact]
    public async Task Many_to_many_link_row_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();
        fixture.SeedAuthorsAndBooks();

        await using var context = fixture.CreateContext();
        var author = await context.Authors.SingleAsync(a => a.Id == 1);
        var book = await context.Books.SingleAsync(b => b.Id == 2);
        author.Books.Add(book);
        await context.SaveChangesAsync();

        await using var verify = fixture.CreateContext();
        var loaded = await verify.Authors.Include(a => a.Books).SingleAsync(a => a.Id == 1);
        Assert.Contains(loaded.Books, b => b.Id == 2);
    }
}
