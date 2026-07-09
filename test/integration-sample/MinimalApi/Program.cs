using IntegrationSample.MinimalApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Xugu");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = Environment.GetEnvironmentVariable("XUGU_CONNECTION");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Set ConnectionStrings:Xugu or XUGU_CONNECTION environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseXugu(connectionString));

var app = builder.Build();

if (string.Equals(Environment.GetEnvironmentVariable("INTEGRATION_SMOKE"), "true", StringComparison.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await EnsureIntegrationSampleSchemaAsync(db);
}

app.MapGet("/health", async (AppDbContext db, CancellationToken ct) =>
{
    var ok = await db.Database.CanConnectAsync(ct);
    return ok ? Results.Ok(new { status = "healthy", database = "xugu" }) : Results.StatusCode(503);
});

app.MapGet("/api/items", async (AppDbContext db, CancellationToken ct) =>
    await db.Items.AsNoTracking().OrderBy(i => i.Id).ToListAsync(ct));

app.MapGet("/api/items/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
    await db.Items.FindAsync([id], ct) is { } item ? Results.Ok(item) : Results.NotFound());

app.MapPost("/api/items", async (CreateItemRequest request, AppDbContext db, CancellationToken ct) =>
{
    var entity = new Item { Name = request.Name };
    db.Items.Add(entity);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/items/{entity.Id}", entity);
});

app.MapPut("/api/items/{id:int}", async (int id, UpdateItemRequest request, AppDbContext db, CancellationToken ct) =>
{
    var entity = await db.Items.FindAsync([id], ct);
    if (entity is null) return Results.NotFound();
    entity.Name = request.Name;
    await db.SaveChangesAsync(ct);
    return Results.Ok(entity);
});

app.MapDelete("/api/items/{id:int}", async (int id, AppDbContext db, CancellationToken ct) =>
{
    var entity = await db.Items.FindAsync([id], ct);
    if (entity is null) return Results.NotFound();
    db.Items.Remove(entity);
    await db.SaveChangesAsync(ct);
    return Results.NoContent();
});

app.Run();

static async Task EnsureIntegrationSampleSchemaAsync(AppDbContext db)
{
    try
    {
        await db.Database.ExecuteSqlRawAsync("DROP TABLE INT_SAMPLE_ITEMS CASCADE");
    }
    catch
    {
        // Table may not exist on first run.
    }

    var script = db.Database.GenerateCreateScript();
    foreach (var statement in script.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (statement.Length > 0)
        {
            await db.Database.ExecuteSqlRawAsync(statement);
        }
    }
}

public record CreateItemRequest(string Name);
public record UpdateItemRequest(string Name);
