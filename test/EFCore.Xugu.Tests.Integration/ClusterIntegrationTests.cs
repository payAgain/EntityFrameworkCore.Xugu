using Microsoft.EntityFrameworkCore.Xugu.Infrastructure;
using Microsoft.EntityFrameworkCore.Xugu.Tests.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Xugu.Tests;

/// <summary>
/// Opt-in multi-node cluster smoke (same or different hosts).
/// Requires <c>XUGU_CLUSTER_PORTS</c> (e.g. <c>5287,5288,5289</c>) plus a primary
/// <c>XUGU_CONNECTION_STRING</c>. Docs: <c>SHOW CLUSTERS</c> /
/// <c>reference/system-configuration-parameter/builtin-variable/clusters.md</c>.
/// </summary>
/// <remarks>
/// C# driver <c>IPS=</c> rotation assumes multiple IPs sharing one <c>PORT</c>.
/// Same-host multi-port clusters must open each listen port separately.
/// </remarks>
[Trait("Category", XuguDialectTestConfiguration.NativeDialectCategory)]
[Trait("Category", "QualityMatrix")]
[Trait("Category", "Cluster")]
public class ClusterIntegrationTests
{
    [SkippableFact]
    public void Each_cluster_listen_port_opens()
    {
        XuguTestConnection.SkipIfClusterNotConfigured();

        foreach (var cs in XuguTestConnection.ClusterNodeConnectionStrings)
        {
            using var connection = XuguTestConnection.OpenConnection(cs);
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM dual";
            Assert.Equal(1, Convert.ToInt32(command.ExecuteScalar()));
        }
    }

    [SkippableFact]
    public void Show_clusters_returns_all_nodes_from_each_listen_port()
    {
        XuguTestConnection.SkipIfClusterNotConfigured();

        var expected = XuguTestConnection.ClusterNodeConnectionStrings.Count;
        Assert.True(expected >= 2);

        foreach (var cs in XuguTestConnection.ClusterNodeConnectionStrings)
        {
            using var connection = XuguTestConnection.OpenConnection(cs);
            using var command = connection.CreateCommand();
            command.CommandText = "SHOW CLUSTERS";
            using var reader = command.ExecuteReader();

            var nodeIds = new HashSet<int>();
            while (reader.Read())
            {
                nodeIds.Add(Convert.ToInt32(reader["NODE_ID"]));
            }

            Assert.True(
                nodeIds.Count >= expected,
                $"Expected at least {expected} NODE_ID rows from SHOW CLUSTERS; got {nodeIds.Count} via {cs}");
        }
    }

    [SkippableFact]
    public void Table_written_on_first_node_is_visible_on_other_nodes()
    {
        XuguTestConnection.SkipIfClusterNotConfigured();

        var nodes = XuguTestConnection.ClusterNodeConnectionStrings;
        var table = $"T_EF_CL_{Guid.NewGuid():N}"[..24];
        var marker = $"m-{Guid.NewGuid():N}"[..12];

        try
        {
            using (var writer = XuguTestConnection.OpenConnection(nodes[0]))
            {
                Execute(writer, $"""
                    CREATE TABLE {table} (
                        ID INT PRIMARY KEY,
                        NAME VARCHAR(50)
                    )
                    """);
                Execute(writer, $"INSERT INTO {table} VALUES (1, '{marker}')");
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                using var readerConn = XuguTestConnection.OpenConnection(nodes[i]);
                using var command = readerConn.CreateCommand();
                command.CommandText = $"SELECT NAME FROM {table} WHERE ID = 1";
                var name = Convert.ToString(command.ExecuteScalar());
                Assert.Equal(marker, name);
            }
        }
        finally
        {
            try
            {
                using var cleanup = XuguTestConnection.OpenConnection(nodes[0]);
                Execute(cleanup, $"DROP TABLE IF EXISTS {table}");
            }
            catch
            {
                // best-effort
            }
        }
    }

    [SkippableFact]
    public void Ef_savechanges_on_node1_is_queryable_from_other_nodes()
    {
        XuguTestConnection.SkipIfClusterNotConfigured();

        var nodes = XuguTestConnection.ClusterNodeConnectionStrings;
        var table = $"T_EF_CLB_{Guid.NewGuid():N}"[..24];
        var title = $"ef-{Guid.NewGuid():N}"[..16];

        try
        {
            using (var bootstrap = XuguTestConnection.OpenConnection(nodes[0]))
            {
                Execute(bootstrap, $"""
                    CREATE TABLE {table} (
                        ID INT NOT NULL,
                        TITLE VARCHAR(100)
                    )
                    """);
                Execute(
                    bootstrap,
                    $"ALTER TABLE {table} ALTER COLUMN ID INTEGER IDENTITY(1, 1) PRIMARY KEY");
            }

            using (var writeCtx = CreateContext(nodes[0], table))
            {
                writeCtx.Blogs.Add(new Blog { Title = title });
                writeCtx.SaveChanges();
            }

            for (var i = 1; i < nodes.Count; i++)
            {
                using var readCtx = CreateContext(nodes[i], table);
                var found = readCtx.Blogs.Single(b => b.Title == title);
                Assert.Equal(title, found.Title);
            }
        }
        finally
        {
            try
            {
                using var cleanup = XuguTestConnection.OpenConnection(nodes[0]);
                Execute(cleanup, $"DROP TABLE IF EXISTS {table}");
            }
            catch
            {
                // best-effort
            }
        }
    }

    private static BlogContext CreateContext(string connectionString, string tableName)
    {
        var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
        optionsBuilder.UseXugu(
            connectionString,
            XuguServerVersion.Default,
            xugu =>
            {
                if (XuguDialectTestConfiguration.UseCompatibleMode)
                {
                    xugu.SetCompatibleModeOnOpen();
                }
                else
                {
                    xugu.DisableCompatibleModeOnOpen();
                }
            });

        return new BlogContext(optionsBuilder.Options, tableName);
    }

    private static void Execute(XuguClient.XGConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    private sealed class BlogContext(DbContextOptions<BlogContext> options, string tableName) : DbContext(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.ToTable(tableName);
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("ID");
                entity.Property(e => e.Title).HasColumnName("TITLE").HasMaxLength(100);
            });
        }
    }
}
