using System.Collections.Immutable;
using static System.Collections.Immutable.ImmutableArray<DiceGame.GameAggregate.Player>;
using static System.Collections.Immutable.ImmutableArray<DiceGame.GameAggregate.DiceValue>;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.GameAggregate;

public record GameState {
  public int       Id        { get; private init; }
  public GameStage GameStage { get; private init; }
  public Score     TurnScore { get; private init; } = new(0);

  public ImmutableArray<Player>    Players     { get; private init; } = ImmutableArray<Player>.Empty;
  public ImmutableArray<DiceValue> TableCenter { get; private init; } = ImmutableArray<DiceValue>.Empty;
  public ImmutableArray<Roll>      Rolls       { get; private init; } = ImmutableArray<Roll>.Empty;
  public ImmutableArray<DiceValue> DiceKept    { get; private init; } = ImmutableArray<DiceValue>.Empty;

  public ImmutableDictionary<PlayerId, int> ScoreTable { get; private init; } =
    ImmutableDictionary<PlayerId, int>.Empty;

  public   Roll?  LastRoll                        => Rolls.LastOrDefault();
  public   Score  GameScoreFor(PlayerId playerId) => new(ScoreTable[playerId]);
  public   Player GetPlayer(int         id)       => Players.Single(p => p.Id == id);
  internal int    PlayerInTurn                    => Players[0].Id;
  public   bool   IsFirstRoll                     => LastRoll is null;


  public GameState When(object @event) {
    var state = @event switch
    {
      GameStarted e  => HandleGameStarted(this, e),
      PlayerJoined e => HandlePlayerJoined(this, e),
      DiceRolled e   => HandleDiceRolled(this, e),
      TurnPassed e   => HandleTurnPassed(this, e),
      DiceKept e     => HandleDiceKept(this, e),
      _              => this
    };

    return state;
  }

  private static GameState HandleDiceKept(GameState state, DiceKept e)
    => state with
    {
      DiceKept = state.DiceKept.AddRange(Dice.FromValues(e.Dice).DiceValues),
      TurnScore = e.NewTurnScore,
      TableCenter = state.TableCenter.RemoveRange(Dice.FromValues(e.Dice).DiceValues)
    };

  private static GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with
    {
      Players = e.PlayerOrder,
      ScoreTable = state.ScoreTable
        .SetItem(
          e.PlayerId,
          e.GameScore)
    };

  private static GameState HandleDiceRolled(GameState state, DiceRolled e)
    => state with
    {
      Rolls = state.Rolls.Add(new Roll(new Dice(e.Dice.ToDiceValues()))),
      TurnScore = e.TurnScore,
      TableCenter = ImmutableArray<DiceValue>.Empty.AddRange(e.Dice.ToDiceValues())
    };

  private static GameState HandlePlayerJoined(GameState state, PlayerJoined playerJoined)
    => state with
    {
      Players = state.Players.Add(new Player(playerJoined.Id, playerJoined.Name)),
      ScoreTable = state.ScoreTable.Add(playerJoined.Id, 0)
    };

  private static GameState HandleGameStarted(GameState gameState, GameStarted e)
    => gameState with
    {
      Id = e.Id,
      GameStage = GameStage.Started
    };
}

public record Score(int Value) {
  public static implicit operator int(Score score) => score.Value;

  public static implicit operator Score(int score) => new Score(score);
}

public record Roll(Dice Dice);