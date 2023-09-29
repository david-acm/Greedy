using System.Collections.Immutable;
using static DiceGame.GameEvents;

namespace DiceGame;

public record GameState(
  int Id,
  GameStage GameStage,
  ImmutableArray<Player> Players,
  int PlayerInTurn
) {
  public GameState When(object @event) {
    var state = @event switch
    {
      GameStarted e => HandleGameStarted(this, e),
      PlayerJoined e => HandlePlayerJoined(this, e),
      DiceThrown e => HandleDiceThrown(this, e),
      _ => this
    };

    return state;
  }

  private GameState HandleDiceThrown(GameState gameState, DiceThrown diceThrown) {
    return gameState with
    {
      Throws = Throws.Add(new Throw(
        diceThrown.PlayerId,
        new Dice(
          (DiceValue)diceThrown.Die1,
          (DiceValue)diceThrown.Die2,
          (DiceValue)diceThrown.Die3,
          (DiceValue)diceThrown.Die4,
          (DiceValue)diceThrown.Die5,
          (DiceValue)diceThrown.Die6
        ))),
      Players = RotatePlayer(diceThrown)
    };
  }

  private ImmutableArray<Player> RotatePlayer(DiceThrown diceThrown) {
    var player = GetPlayer(diceThrown);
    var newPlayerList = Players.Remove(player).Add(player);
    return ImmutableArray<Player>.Empty.AddRange(newPlayerList);
  }

  private Player GetPlayer(DiceThrown diceThrown) {
    return Players.Single(p => p.Id == diceThrown.PlayerId);
  }

  public ImmutableArray<Throw> Throws { get; private set; } = ImmutableArray<Throw>.Empty;
  internal int PlayerInTurn => Players[0].Id;

  private GameState HandlePlayerJoined(GameState gameState, PlayerJoined playerJoined)
    => gameState with
    {
      Players = Players.Add(new Player(playerJoined.Id, playerJoined.Name))
    };

  private GameState HandleGameStarted(GameState gameState, GameStarted e) =>
    gameState with
    {
      Id = e.Id,
      GameStage = GameStage.Started
    };
}

public record Throw(int PlayerId, Dice Dice);