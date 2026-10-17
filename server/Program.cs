using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Hub") ?? "Data Source=game-session-hub.db";
builder.Services.AddDbContext<HubDb>(options => options.UseSqlite(connectionString));
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins(
    "http://localhost:5173", "http://127.0.0.1:5173",
    "http://localhost:5175", "http://127.0.0.1:5175").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails();
var app = builder.Build();
app.UseExceptionHandler();
app.UseCors();
using (var scope = app.Services.CreateScope()) await scope.ServiceProvider.GetRequiredService<HubDb>().Database.EnsureCreatedAsync();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
var lobbies = app.MapGroup("/api/lobbies");
lobbies.MapGet("/", async (HubDb db, CancellationToken ct) => Results.Ok((await db.Lobbies.Include(lobby => lobby.Members).OrderByDescending(lobby => lobby.CreatedAtUtc).ToListAsync(ct)).Select(ToResponse)));
lobbies.MapPost("/", async (CreateLobbyRequest request, HubDb db, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length is < 3 or > 40) return Results.ValidationProblem(new Dictionary<string, string[]> { ["name"] = ["Name must contain 3 to 40 characters."] });
    if (request.MaxPlayers is < 2 or > 8) return Results.ValidationProblem(new Dictionary<string, string[]> { ["maxPlayers"] = ["Max players must be between 2 and 8."] });
    var lobby = new Lobby { Name = request.Name.Trim(), Region = string.IsNullOrWhiteSpace(request.Region) ? "EU-West" : request.Region.Trim(), MaxPlayers = request.MaxPlayers };
    db.Lobbies.Add(lobby); await db.SaveChangesAsync(ct); return Results.Created($"/api/lobbies/{lobby.Id}", ToResponse(lobby));
});
lobbies.MapPost("/{id:guid}/members", async (Guid id, JoinLobbyRequest request, HubDb db, CancellationToken ct) =>
{
    var name = request.PlayerName?.Trim(); if (string.IsNullOrWhiteSpace(name)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["playerName"] = ["Player name is required."] });
    var lobby = await db.Lobbies.Include(item => item.Members).SingleOrDefaultAsync(item => item.Id == id, ct);
    if (lobby is null) return Results.NotFound(); if (lobby.Status != LobbyStatus.Open) return Results.Conflict(new { message = "This lobby is no longer open." }); if (lobby.Members.Count >= lobby.MaxPlayers) return Results.Conflict(new { message = "This lobby is full." }); if (lobby.Members.Any(member => member.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase))) return Results.Conflict(new { message = "Player is already in this lobby." });
    db.Members.Add(new LobbyMember { LobbyId = lobby.Id, PlayerName = name }); await db.SaveChangesAsync(ct);
    var updatedLobby = await db.Lobbies.Include(item => item.Members).SingleAsync(item => item.Id == id, ct);
    return Results.Ok(ToResponse(updatedLobby));
});
lobbies.MapPost("/{id:guid}/start", async (Guid id, HubDb db, CancellationToken ct) =>
{
    var lobby = await db.Lobbies.Include(item => item.Members).SingleOrDefaultAsync(item => item.Id == id, ct); if (lobby is null) return Results.NotFound(); if (lobby.Members.Count < 2) return Results.Conflict(new { message = "At least two players are needed to start." }); lobby.Status = LobbyStatus.InProgress; await db.SaveChangesAsync(ct); return Results.Ok(ToResponse(lobby));
});
app.Run();
static LobbyResponse ToResponse(Lobby lobby) => new(lobby.Id, lobby.Name, lobby.Region, lobby.Status.ToString(), lobby.MaxPlayers, lobby.Members.Select(member => member.PlayerName).Order().ToList(), lobby.CreatedAtUtc);
public sealed class HubDb(DbContextOptions<HubDb> options) : DbContext(options) { public DbSet<Lobby> Lobbies => Set<Lobby>(); public DbSet<LobbyMember> Members => Set<LobbyMember>(); protected override void OnModelCreating(ModelBuilder builder) { builder.Entity<Lobby>().HasMany(lobby => lobby.Members).WithOne().HasForeignKey(member => member.LobbyId).OnDelete(DeleteBehavior.Cascade); builder.Entity<LobbyMember>().HasIndex(member => new { member.LobbyId, member.PlayerName }).IsUnique(); } }
public enum LobbyStatus { Open, InProgress }
public sealed class Lobby { public Guid Id { get; set; } = Guid.NewGuid(); public string Name { get; set; } = string.Empty; public string Region { get; set; } = string.Empty; public int MaxPlayers { get; set; } = 4; public LobbyStatus Status { get; set; } = LobbyStatus.Open; public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow; public List<LobbyMember> Members { get; set; } = []; }
public sealed class LobbyMember { public Guid Id { get; set; } = Guid.NewGuid(); public Guid LobbyId { get; set; } public string PlayerName { get; set; } = string.Empty; }
public sealed record CreateLobbyRequest(string? Name, string? Region, int MaxPlayers);
public sealed record JoinLobbyRequest(string? PlayerName);
public sealed record LobbyResponse(Guid Id, string Name, string Region, string Status, int MaxPlayers, IReadOnlyList<string> Members, DateTime CreatedAtUtc);

public partial class Program { }
