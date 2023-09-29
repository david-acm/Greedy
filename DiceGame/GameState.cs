using System.Collections.Immutable;
using static DiceGame.GameEvents;

namespace DiceGame;

public record GameState(int Id, GameStage GameStage, ImmutableArray<Player> Players) {
  public GameState When(object @event) {
    var state = @event switch
    {
      GameStarted e => HandleGameStarted(this, e),
      PlayerJoined e => HandlePlayerJoined(this, e),
      DiceThrown e => HandleDiceTrown(this, e),
      _ => this
    };

    return state;
  }

  private GameState HandleDiceTrown(GameState gameState, DiceThrown diceThrown) =>
    gameState with
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
        )))
    };

  public ImmutableArray<Throw> Throws { get; private set; } = ImmutableArray<Throw>.Empty;

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