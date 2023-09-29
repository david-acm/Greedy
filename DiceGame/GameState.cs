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
      TurnPassed e => HandleTurnPassed(this, e),
      
      _ => this
    };

    return state;
  }

  private GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with { Players = e.RotatedPlayers };

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
    };
  }
  
  public Player GetPlayer(int id) => Players.Single(p => p.Id == id);

  public ImmutableArray<Throw> Throws { get; private set; } = ImmutableArray<Throw>.Empty;
  internal int PlayerInTurn => Players[0].Id;

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

public record Throw(int PlayerId, Dice Dice);