using Microsoft.AspNetCore.DataProtection;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);


var keyPath = Path.Combine(
    Environment.GetEnvironmentVariable("HOME") ?? "D:\\home", "data", "keys");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .SetApplicationName("StockApp");

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7079);
});
builder.WebHost.UseUrls("http://*:7079");

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

app.MapGet("/testdb", async (IConfiguration config) =>
{
    try
    {
        var connStr = config.GetConnectionString("PostgresDB")
              ?? Environment.GetEnvironmentVariable("PostgresDB");

        using var conn = new NpgsqlConnection(connStr);
        await conn.OpenAsync();
        return Results.Ok("Database connection successful!");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database connection failed: {ex.Message}");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
