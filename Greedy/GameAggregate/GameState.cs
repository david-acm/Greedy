using System.Collections.Immutable;
using Eventuous;
using static System.Collections.Immutable.ImmutableArray<Greedy.GameAggregate.Player>;
using static System.Collections.Immutable.ImmutableArray<Greedy.GameAggregate.DiceValue>;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.GameAggregate;

public record GameState : State<GameState> {
  public GameId    Id        { get; private init; }
  public GameStage GameStage { get; private init; }
  public Score     TurnScore { get; private init; } = new(0);

  public ImmutableArray<Player>    Players     { get; private init; } = ImmutableArray<Player>.Empty;
  public ImmutableArray<DiceValue> TableCenter { get; private init; } = ImmutableArray<DiceValue>.Empty;
  public ImmutableArray<DiceRoll>      Rolls       { get; private init; } = ImmutableArray<DiceRoll>.Empty;
  public ImmutableArray<DiceValue> DiceKept    { get; private init; } = ImmutableArray<DiceValue>.Empty;

  public ImmutableDictionary<int, int> ScoreTable { get; private init; } =
    ImmutableDictionary<int, int>.Empty;

  public   DiceRoll?  LastRoll                        => Rolls.LastOrDefault();
  public   Score  GameScoreFor(PlayerId playerId) => new(ScoreTable[playerId]);
  public   Player GetPlayer(int         id)       => Players.Single(p => p.Id == id);
  internal int    PlayerInTurn                    => Players[0].Id;
  public   bool   IsFirstRoll                     => LastRoll is null;


  public GameState() {
    On<GameStarted>(HandleGameStarted);
    On<PlayerJoined>(HandlePlayerJoined);
    On<DiceRolled>(HandleDiceRolled);
    On<TurnPassed>(HandleTurnPassed);
    On<DiceKept>(HandleDiceKept);
  }

  private static GameState HandleDiceKept(GameState state, DiceKept e)
    => state with
    {
      DiceKept = state.DiceKept.AddRange(Dice.FromValues(e.Dice).DiceValues),
      TurnScore = e.NewTurnScore,
      TableCenter = ImmutableArray<DiceValue>.Empty.AddRange(e.TableCenter.ToDiceValues())
    };

  private static GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with
    {
      Players = e.PlayerOrder,
      ScoreTable = state.ScoreTable
        .SetItem(
          e.PlayerId,
          e.GameScore),
      TableCenter = state.TableCenter.AddRange(state.DiceKept)
    };

  private static GameState HandleDiceRolled(GameState state, DiceRolled e)
    => state with
    {
      Rolls = state.Rolls.Add(new DiceRoll(new Dice(e.Dice.ToDiceValues()))),
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

public record DiceRoll(Dice Dice);

public record GameId(int Id) : AggregateId($"{Id}") {
  public static implicit operator GameId(int id) => new(id);
  public static implicit operator int(GameId id) => id.Id;
}