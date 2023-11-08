using Microsoft.AspNetCore.Components;
using DiceValue = Greedy.Spa.Components.DiceValue;

namespace Greedy.Spa.Pages;

public partial class Game {
  private readonly string _playerName = "David";

  private int _gameId;
  private int _playerId;

  [Inject] public IGameService? GameService { get; set; }
  private         string?       Values      { get; set; } = "1 2 3 4 5 6";

  private List<DiceValue> DiceValues => Values?
    .Split(' ')?
    .Where(d => { return int.TryParse(d, out int v); })?
    .Select(v =>
    {
      var parsed = DiceValue.TryFromValue(int.Parse(v), out DiceValue value);
      return parsed ? value : DiceValue.One;
    })?
    .ToList() ?? new();

  protected override async Task OnInitializedAsync() {
    var random = new Random();
    _gameId = random.Next(0, 999);
    await GameService.StartGameAsync(_gameId);
    _playerId = random.Next(0, 100);
    await GameService.JoinPlayerAsync(_gameId, _playerId, _playerName);
  }

  private async Task RollAsync() {
    var dice = await GameService.RollDiceAsync(_gameId, _playerId);
    Values = string.Empty;
    Values = string.Concat(dice.Select(d => $"{d.Value} "));

    Values = Values.Trim();
  }
}