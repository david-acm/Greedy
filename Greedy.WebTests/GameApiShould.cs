using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Greedy.WebApi.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Greedy.WebTests;

public class GameApiShould : IClassFixture<GameApi> {
  private readonly HttpClient _client;

  public GameApiShould(GameApi factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task AllowPlayerToRollDice()
  {
    // Arrange
    const int gameId = 208;

    // Act
    await _client.PostAndEnsureOkStatusCode(
      "/games",
      new { Id = gameId });

    await _client.PostAndEnsureOkStatusCode(
      "/players",
      JoinPlayerRequest(1, "David"));

    await _client.PostAndEnsureOkStatusCode(
      "/players",
      JoinPlayerRequest(2, "Allison"));

    var roll = await _client.PostAndEnsureOkStatusCode(
      "/diceRolls",
      RollDice(1));

    await _client.PostAndEnsureOkStatusCode(
      "/diceKeeps",
      KeepDice(1, new[] { 1 }));

    // Assert
    object JoinPlayerRequest(int playerId, string playerName)
    {
      return new { GameId = gameId, PlayerId = playerId, PlayerName = playerName };
    }

    object RollDice(int playerId)
    {
      return new { GameId = gameId, PlayerId = playerId };
    }

    object KeepDice(int playerId, int[] diceValues)
    {
      return new { GameId = gameId, PlayerId = playerId, DiceValues = diceValues };
    }
  }
}

public static class HttpClientExtensions {
  public static async Task<HttpResponseMessage> PostAndEnsureOkStatusCode(this HttpClient client, string route,
    object                                                                                body)
  {
    var    result  = await client.PostAsJsonAsync(route, body);
    string content = await result.Content.ReadAsStringAsync();

    result.IsSuccessStatusCode.Should().
      BeTrue($"Status code returned was: {result.StatusCode}, with reason: {result.ReasonPhrase} {content}");

    return result;
  }
}

public class GameApi : WebApplicationFactory<GameService> {
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    var container = new ContainerBuilder().WithImage("ghcr.io/eventstore/eventstore:21.10.0-alpha-arm64v8").
      WithPortBinding(1113).WithPortBinding(2113).WithEnvironment(new Dictionary<string, string>
      {
        { "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true" },
        { "EVENTSTORE_INSECURE", "true" },
        { "EVENTSTORE_CLUSTER_SIZE", "1" },
        { "EVENTSTORE_EXT_TCP_PORT", "1113" },
        { "EVENTSTORE_HTTP_PORT", "2113" },
        { "EVENTSTORE_ENABLE_EXTERNAL_TCP", "true" },
        { "EVENTSTORE_RUN_PROJECTIONS", "all" },
        { "EVENTSTORE_START_STANDARD_PROJECTIONS", "true" },
        { "PATH", "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin" },
        { "ASPNETCORE_URLS", "http://+:80" },
        { "DOTNET_RUNNING_IN_CONTAINER", "true" }
      }).WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(2113))).
      WithAutoRemove(false).Build();
    container.StartAsync().GetAwaiter().GetResult();

    base.ConfigureWebHost(builder);
  }
}