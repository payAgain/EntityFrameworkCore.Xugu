using Microsoft.EntityFrameworkCore.Xugu.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Phase 9.T27 — MusicStoreMySqlTest subset (store browse scenario).
/// </summary>
[Collection("XuguMusicStore")]
public class MusicStoreTests(MusicStoreFixture fixture)
{
    [SkippableFact]
    public async Task Index_returns_all_genres()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            SeedGenres(context, genreCount: 5, albumsPerGenre: 1);
        }

        await using var read = fixture.CreateContext();
        var genres = await read.Genres.OrderBy(g => g.Name).ToListAsync();
        Assert.Equal(5, genres.Count);
    }

    [SkippableFact]
    public async Task Browse_genre_returns_albums()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            SeedGenres(context, genreCount: 3, albumsPerGenre: 2);
        }

        await using var read = fixture.CreateContext();
        var genre = await read.Genres.Include(g => g.Albums).SingleAsync(g => g.Name == "Genre 2");
        Assert.Equal(2, genre.Albums.Count);
    }

    [SkippableFact]
    public async Task Album_details_include_artist_and_genre()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        int albumId;
        await using (var context = fixture.CreateContext())
        {
            var genres = SeedGenres(context, genreCount: 1, albumsPerGenre: 1);
            albumId = genres[0].Albums[0].AlbumId;
        }

        await using var read = fixture.CreateContext();
        var album = await read.Albums
            .Include(a => a.Genre)
            .Include(a => a.Artist)
            .SingleAsync(a => a.AlbumId == albumId);

        Assert.NotNull(album.Genre);
        Assert.NotNull(album.Artist);
        Assert.Equal("Artist1", album.Artist!.Name);
    }

    [SkippableFact]
    public async Task Add_album_to_genre_persists()
    {
        XuguTestConnection.SkipIfUnavailable();
        fixture.ResetStore();

        await using (var context = fixture.CreateContext())
        {
            var artist = new Artist { Name = "New Artist" };
            var genre = new Genre { Name = "Jazz", Albums = [new Album { Title = "Blue", Artist = artist }] };
            context.Add(genre);
            await context.SaveChangesAsync();
        }

        await using var read = fixture.CreateContext();
        Assert.Single(await read.Albums.Where(a => a.Title == "Blue").ToListAsync());
    }

    [Fact]
    public void Music_store_model_maps_relationships()
    {
        using var context = fixture.CreateContext();
        var genre = context.Model.FindEntityType(typeof(Genre))!;
        Assert.NotNull(genre.FindNavigation(nameof(Genre.Albums)));
    }

    private static List<Genre> SeedGenres(MusicStoreContext context, int genreCount, int albumsPerGenre)
    {
        var artist = new Artist { Name = "Artist1" };
        var genres = Enumerable.Range(1, genreCount).Select(i => new Genre
        {
            Name = $"Genre {i}",
            Albums = Enumerable.Range(1, albumsPerGenre)
                .Select(_ => new Album { Title = "Greatest Hits", Artist = artist })
                .ToList()
        }).ToList();

        context.AddRange(genres);
        context.SaveChanges();
        return genres;
    }

    public sealed class Artist
    {
        public int ArtistId { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Album> Albums { get; set; } = [];
    }

    public sealed class Genre
    {
        public int GenreId { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<Album> Albums { get; set; } = [];
    }

    public sealed class Album
    {
        public int AlbumId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int GenreId { get; set; }

        public int ArtistId { get; set; }

        public Genre? Genre { get; set; }

        public Artist? Artist { get; set; }
    }

    public sealed class MusicStoreContext : DbContext, IXuguStoreBoundContext
    {
        private readonly XuguTestStore _store;

        public MusicStoreContext(DbContextOptions<MusicStoreContext> options, XuguTestStore store)
            : base(options)
        {
            _store = store;
        }

        public string TableNamePrefix => _store.TableNamePrefix;

        public DbSet<Artist> Artists => Set<Artist>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<Album> Albums => Set<Album>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var artistTable = _store.FormatTableName("MsArtist");
            var genreTable = _store.FormatTableName("MsGenre");
            var albumTable = _store.FormatTableName("MsAlbum");

            modelBuilder.Entity<Artist>(entity =>
            {
                entity.ToTable(artistTable);
                entity.HasKey(e => e.ArtistId);
                entity.Property(e => e.ArtistId).HasColumnName("ARTIST_ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
            });

            modelBuilder.Entity<Genre>(entity =>
            {
                entity.ToTable(genreTable);
                entity.HasKey(e => e.GenreId);
                entity.Property(e => e.GenreId).HasColumnName("GENRE_ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Name).HasColumnName("NAME").HasMaxLength(200);
                entity.HasMany(e => e.Albums).WithOne(e => e.Genre).HasForeignKey(e => e.GenreId);
            });

            modelBuilder.Entity<Album>(entity =>
            {
                entity.ToTable(albumTable);
                entity.HasKey(e => e.AlbumId);
                entity.Property(e => e.AlbumId).HasColumnName("ALBUM_ID").ValueGeneratedOnAdd();
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(200);
                entity.Property(e => e.GenreId).HasColumnName("GENRE_ID");
                entity.Property(e => e.ArtistId).HasColumnName("ARTIST_ID");
                entity.HasOne(e => e.Artist).WithMany(e => e.Albums).HasForeignKey(e => e.ArtistId);
            });
        }
    }
}

public sealed class MusicStoreFixture : XuguSharedStoreFixture<MusicStoreTests.MusicStoreContext>
{
    protected override string StoreName => "MusicStore";

    protected override MusicStoreTests.MusicStoreContext CreateContext(
        DbContextOptions<MusicStoreTests.MusicStoreContext> options)
        => new(options, TestStore);

    protected override Task OnStoreInitializedAsync()
    {
        if (XuguTestConnection.IsAvailable())
        {
            ResetStore();
        }

        return Task.CompletedTask;
    }

    public void ResetStore()
    {
        var album = TestStore.FormatAndTrackTable("MsAlbum");
        var genre = TestStore.FormatAndTrackTable("MsGenre");
        var artist = TestStore.FormatAndTrackTable("MsArtist");

        TestStore.TryExecuteNonQuery($"DROP TABLE {album} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {genre} CASCADE");
        TestStore.TryExecuteNonQuery($"DROP TABLE {artist} CASCADE");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {artist} (
                ARTIST_ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {artist} ALTER COLUMN ARTIST_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {genre} (
                GENRE_ID INTEGER NOT NULL,
                NAME VARCHAR(200) NOT NULL
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {genre} ALTER COLUMN GENRE_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");

        TestStore.ExecuteNonQuery(
            $"""
            CREATE TABLE {album} (
                ALBUM_ID INTEGER NOT NULL,
                TITLE VARCHAR(200) NOT NULL,
                GENRE_ID INTEGER NOT NULL,
                ARTIST_ID INTEGER NOT NULL,
                FOREIGN KEY (GENRE_ID) REFERENCES {genre}(GENRE_ID),
                FOREIGN KEY (ARTIST_ID) REFERENCES {artist}(ARTIST_ID)
            )
            """);
        TestStore.ExecuteNonQuery($"ALTER TABLE {album} ALTER COLUMN ALBUM_ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
    }
}
