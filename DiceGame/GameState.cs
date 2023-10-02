using System.Collections.Immutable;
using static DiceGame.GameEvents;

namespace DiceGame;

public record GameState(
  int Id,
  GameStage GameStage,
  ImmutableArray<Player> Players,
  int PlayerInTurn
) {
  public Play? LastThrow
  {
    get { return Throws.LastOrDefault(); }
  }

  public GameState When(object @event) {
    var state = @event switch
    {
      GameStarted e => HandleGameStarted(this, e),
      PlayerJoined e => HandlePlayerJoined(this, e),
      DiceThrown e => HandleDiceThrown(this, e),
      TurnPassed e => HandleTurnPassed(this, e),
      DiceKept e => HandleDiceKept(this, e),
      _ => this
    };

    return state;
  }

  private GameState HandleDiceKept(GameState state, DiceKept diceKept) {
    return state with
    {
      DiceKept = DiceKept.AddRange(Dice.FromValues(diceKept.Dice).DiceValues)
    };
  }

  private GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with { Players = e.RotatedPlayers };

  private GameState HandleDiceThrown(GameState gameState, DiceThrown diceThrown) {
    return gameState with
    {
      Throws = Throws.Add(new Play(
        diceThrown.PlayerId,
        new Dice(diceThrown.Dice.Select(d => (DiceValue)d)))),
    };
  }

  public Player GetPlayer(int id) => Players.Single(p => p.Id == id);

  public ImmutableArray<Play> Throws { get; private set; } = ImmutableArray<Play>.Empty;
  public ImmutableArray<DiceValue> DiceKept { get; private set; } = ImmutableArray<DiceValue>.Empty;
  internal int PlayerInTurn => Players[0].Id;

  public IEnumerable<DiceValue> TableCenter => LastThrow is null
    ? ImmutableArray<DiceValue>.Empty
    : ImmutableArray<DiceValue>.Empty.AddRange(LastThrow.Dice.DiceValues).RemoveRange(DiceKept);

  private GameState HandlePlayerJoined(GameState gameState, PlayerJoined playerJoined)
    => gameState with
    {
      Players = Players.Add(new Player(playerJoined.Id, playerJoined.Name))
    };

  private GameState HandleGameStarted(
    GameState gameState,
    GameStarted e
  ) => gameState with
  {
    Id = e.Id,
    GameStage = GameStage.Started
  };
}

public record Play(int PlayerId, Dice Dice);