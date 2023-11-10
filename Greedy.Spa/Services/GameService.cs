using System.Net.Http.Json;
using System.Text.Json;
using Greedy.Spa.Components;

namespace Greedy.Spa.Services;

public class GameService : IGameService {
  private readonly HttpClient _gameClient;

  public GameService(HttpClient gameClient)
  {
    _gameClient = gameClient;
  }

  public async Task<IList<DiceValue>> RollDiceAsync(int gameId, int playerId)
  {
    var result =
      await _gameClient.PostAsJsonAsync("http://localhost:5276/diceRolls",
        new { GameId = gameId, PlayerId = playerId });

    var options = new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    };

    string stringContent = await result.Content.ReadAsStringAsync();
    var    response      = JsonSerializer.Deserialize<CommandResponse>(stringContent, options);

    var tableCenter = response?.State?.TableCenter?.Select(v => DiceValue.FromValue(int.Parse($"{v.Value}"))).ToList();
    return tableCenter ?? new List<DiceValue>();
  }

  public async Task JoinPlayerAsync(int gameId, int playerId, string playerName) =>
    await _gameClient.PostAsJsonAsync("http://localhost:5276/players",
      new { GameId = gameId, PlayerId = playerId, PlayerName = playerName });

  public async Task StartGameAsync(int gameId) =>
    await _gameClient.PostAsJsonAsync("http://localhost:5276/games", new { Id = gameId });
}