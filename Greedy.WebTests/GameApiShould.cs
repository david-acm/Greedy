namespace Greedy.WebTests;

public class GameApiShould : IClassFixture<GameApiWebAppFactory> {
  private readonly HttpClient _client;

  public GameApiShould(GameApiWebAppFactory factory)
  {
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task AllowPlayerToRollDice()
  {
    // Arrange
    const int gameId = 208;

    // Act
    // Assert
    await _client.PostAndEnsureOkStatusCode(
      "/games",
      new { Id = gameId });

    await _client.PostAndEnsureOkStatusCode(
      "/players",
      JoinPlayerRequest(1, "David", gameId));

    await _client.PostAndEnsureOkStatusCode(
      "/players",
      JoinPlayerRequest(2, "Allison", gameId));

    var roll = await _client.PostAndEnsureOkStatusCode(
      "/diceRolls",
      RollDice(1, gameId));

    await _client.PostAndEnsureOkStatusCode(
      "/diceKeeps",
      KeepDice(1, new[] { 1 }, gameId));
  }

  private static object KeepDice(int playerId, int[] diceValues, int gameId) =>
    new { GameId = gameId, PlayerId = playerId, DiceValues = diceValues };

  private static object RollDice(int playerId, int gameId) =>
    new { GameId = gameId, PlayerId = playerId };

  private static object JoinPlayerRequest(int playerId, string playerName, int gameId) =>
    new { GameId = gameId, PlayerId = playerId, PlayerName = playerName };
}