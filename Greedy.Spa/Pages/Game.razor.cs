using Greedy.Spa.Components;
using Greedy.Spa.Services;
using Microsoft.AspNetCore.Components;

namespace Greedy.Spa.Pages;

public partial class Game {
  private readonly string _playerName = "David";
  private          int    _gameId;
  private          int    _playerId;

  [Inject]
  public IGameService? GameService { get; set; }

  public string? Values { get; set; } = "1 2 3 4 5 6";

  private List<DiceValue> DiceValues =>
    Values?.Split(' ')?.
      Where(d => int.TryParse(d, out int v))?.
      Select(v =>
      {
        bool parsed = DiceValue.TryFromValue(int.Parse(v), out var value);
        return parsed ? value : DiceValue.One;
      })?.
      ToList() ?? new List<DiceValue>();

  private async Task RollAsync()
  {
    var dice = await GameService.RollDiceAsync(_gameId, _playerId);
    Values = string.Empty;
    Values = string.Concat(dice.Select(d => $"{d.Value} "));

    Values = Values.Trim();
  }


  protected override async Task OnInitializedAsync()
  {
    var random = new Random();
    _gameId = random.Next(0, 999);
    await GameService.StartGameAsync(_gameId);
    _playerId = random.Next(0, 100);
    await GameService.JoinPlayerAsync(_gameId, _playerId, _playerName);
  }
}