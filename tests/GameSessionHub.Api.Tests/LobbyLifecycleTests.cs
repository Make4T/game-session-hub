using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace GameSessionHub.Api.Tests;

public sealed class LobbyLifecycleTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"game-session-hub-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseSetting("ConnectionStrings:Hub", $"Data Source={_databasePath};Pooling=False"));
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Lobby_can_be_created_joined_and_started()
    {
        var healthResponse = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);

        var createResponse = await _client.PostAsJsonAsync("/api/lobbies/", new CreateLobbyRequest("Northern Lights", "EU-North", 4));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var lobby = await createResponse.Content.ReadFromJsonAsync<LobbyResponse>();
        Assert.NotNull(lobby);

        var firstJoin = await _client.PostAsJsonAsync($"/api/lobbies/{lobby!.Id}/members", new JoinLobbyRequest("Make4T"));
        var secondJoin = await _client.PostAsJsonAsync($"/api/lobbies/{lobby.Id}/members", new JoinLobbyRequest("Teammate"));
        Assert.Equal(HttpStatusCode.OK, firstJoin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, secondJoin.StatusCode);

        var startResponse = await _client.PostAsync($"/api/lobbies/{lobby.Id}/start", null);
        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);
        var startedLobby = await startResponse.Content.ReadFromJsonAsync<LobbyResponse>();
        Assert.NotNull(startedLobby);
        Assert.Equal("InProgress", startedLobby!.Status);
        Assert.Equal(["Make4T", "Teammate"], startedLobby.Members.Order());
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        File.Delete(_databasePath);
        File.Delete($"{_databasePath}-shm");
        File.Delete($"{_databasePath}-wal");
    }

    private sealed record CreateLobbyRequest(string Name, string Region, int MaxPlayers);
    private sealed record JoinLobbyRequest(string PlayerName);
    private sealed record LobbyResponse(Guid Id, string Name, string Region, string Status, int MaxPlayers, IReadOnlyList<string> Members, DateTime CreatedAtUtc);
}
