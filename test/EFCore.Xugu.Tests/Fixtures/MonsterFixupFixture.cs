using Microsoft.EntityFrameworkCore.Xugu.Tests.Specification;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;

public sealed class MonsterFixupFixture : XuguSharedStoreFixture<MonsterFixupXuguTests.MonsterFixupContext>
{
    protected override string StoreName => "MonsterFixup";

    protected override MonsterFixupXuguTests.MonsterFixupContext CreateContext(
        DbContextOptions<MonsterFixupXuguTests.MonsterFixupContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (!XuguTestConnection.IsAvailable())
        {
            return Task.CompletedTask;
        }

        ResetStore();
        return Task.CompletedTask;
    }

    public void ResetStore()
    {
        foreach (var table in new[]
                 {
                     "MF_BOOK_AUTHOR", "MF_BOOK", "MF_AUTHOR", "MF_NODE", "MF_TAG",
                     "MF_DETAIL", "MF_PRODUCT", "MF_CATEGORY"
                 })
        {
            TestStore.TryExecuteNonQuery($"DROP TABLE {TestStore.FormatTableName(table)} CASCADE");
        }

        var category = TestStore.FormatAndTrackTable("MF_CATEGORY");
        var product = TestStore.FormatAndTrackTable("MF_PRODUCT");
        var detail = TestStore.FormatAndTrackTable("MF_DETAIL");
        var tag = TestStore.FormatAndTrackTable("MF_TAG");
        var node = TestStore.FormatAndTrackTable("MF_NODE");
        var author = TestStore.FormatAndTrackTable("MF_AUTHOR");
        var book = TestStore.FormatAndTrackTable("MF_BOOK");
        var bookAuthor = TestStore.FormatAndTrackTable("MF_BOOK_AUTHOR");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {category} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {category} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {product} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                CATEGORY_ID INTEGER NOT NULL,
                FOREIGN KEY (CATEGORY_ID) REFERENCES {category}(ID) ON DELETE CASCADE
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {product} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {detail} (
                PRODUCT_ID INTEGER NOT NULL PRIMARY KEY,
                NOTES VARCHAR(500) NOT NULL,
                FOREIGN KEY (PRODUCT_ID) REFERENCES {product}(ID) ON DELETE CASCADE
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {tag} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL,
                CATEGORY_ID INTEGER,
                FOREIGN KEY (CATEGORY_ID) REFERENCES {category}(ID)
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {tag} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {node} (
                ID INTEGER NOT NULL PRIMARY KEY,
                NAME VARCHAR(200) NOT NULL,
                PARENT_ID INTEGER,
                FOREIGN KEY (PARENT_ID) REFERENCES {node}(ID)
            )
            """);

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {author} (
                ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {author} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {book} (
                ID INTEGER NOT NULL,
                TITLE VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {book} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery($"""
            CREATE TABLE {bookAuthor} (
                AUTHORS_ID INTEGER NOT NULL,
                BOOKS_ID INTEGER NOT NULL,
                PRIMARY KEY (AUTHORS_ID, BOOKS_ID),
                FOREIGN KEY (AUTHORS_ID) REFERENCES {author}(ID) ON DELETE CASCADE,
                FOREIGN KEY (BOOKS_ID) REFERENCES {book}(ID) ON DELETE CASCADE
            )
            """);
    }

    public void SeedCategories()
    {
        var category = TestStore.FormatTableName("MF_CATEGORY");
        TestStore.ExecuteNonQuery($"INSERT INTO {category} (ID, NAME) VALUES (1, 'C1'), (2, 'C2')");
    }

    public void SeedCategoryWithProducts()
    {
        SeedCategories();
        var product = TestStore.FormatTableName("MF_PRODUCT");
        TestStore.ExecuteNonQuery($"INSERT INTO {product} (ID, NAME, CATEGORY_ID) VALUES (1, 'P1', 1)");
    }

    public void SeedTwoCategoriesWithProduct()
        => SeedCategoryWithProducts();

    public void SeedProducts()
    {
        SeedCategories();
        var product = TestStore.FormatTableName("MF_PRODUCT");
        TestStore.ExecuteNonQuery($"INSERT INTO {product} (ID, NAME, CATEGORY_ID) VALUES (1, 'P1', 1)");
    }

    public void SeedAuthorsAndBooks()
    {
        var author = TestStore.FormatTableName("MF_AUTHOR");
        var book = TestStore.FormatTableName("MF_BOOK");
        TestStore.ExecuteNonQuery($"INSERT INTO {author} (ID, NAME) VALUES (1, 'A1')");
        TestStore.ExecuteNonQuery($"INSERT INTO {book} (ID, TITLE) VALUES (1, 'B1'), (2, 'B2')");
    }

    public void SeedAuthorsAndBooksWithLink()
    {
        SeedAuthorsAndBooks();
        var link = TestStore.FormatTableName("MF_BOOK_AUTHOR");
        TestStore.ExecuteNonQuery($"INSERT INTO {link} (AUTHORS_ID, BOOKS_ID) VALUES (1, 1)");
    }
}
