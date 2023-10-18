using System.Net.Http.Json;
using DotNet.Testcontainers.Builders;
using FluentAssertions;
using Greedy.GameAggregate;
using Greedy.WebApi.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Greedy.WebTests;

public class GameStartShould : IClassFixture<GameApi> {
  private readonly HttpClient _client;

  public GameStartShould(GameApi factory) {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task RecordStartEvent() {
    // Arrange
    var request = new { Id = 208 };
    var games   = "/games";
    var players = "/players";

    // Act
    var result = await _client.PostAsJsonAsync(games, request);
    result = await _client.PostAsJsonAsync(players,
      new { GameId = 208, PlayerId = 1, PlayerName = "David" });
    result = await _client.PostAsJsonAsync(players,
      new { GameId = 208, PlayerId = 2, PlayerName = "Allison" });
    result = await _client.PostAsJsonAsync("diceRolls",
      new { GameId = 208, PlayerId = 1 });
    result = await _client.PostAsJsonAsync("diceKeeps",
      new { GameId = 208, PlayerId = 1, DiceValues = new [] {1} });
    var content = await result.Content.ReadAsStringAsync();

    // Assert
    result.IsSuccessStatusCode.Should()
      .BeTrue(because: $"Status code returned was: {result.StatusCode}, with reason: {result.ReasonPhrase} {content}");
  }
}

public class GameApi : WebApplicationFactory<GameService> {
  protected override void ConfigureWebHost(IWebHostBuilder builder) {
    var container = new ContainerBuilder()
      .WithImage("ghcr.io/eventstore/eventstore:21.10.0-alpha-arm64v8")
      .WithPortBinding(1113, false)
      .WithPortBinding(2113, false)
      .WithEnvironment(new Dictionary<string, string>
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
      })
      .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(2113)))
      .WithAutoRemove(false)
      .Build();
    container.StartAsync().GetAwaiter().GetResult();

    base.ConfigureWebHost(builder);
  }
}