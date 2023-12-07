using System.Collections.Immutable;
using Eventuous;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.GameAggregate;

public record GameState : State<GameState> {
  public GameState()
  {
    On<V1.GameStarted>(HandleGameStarted);
    On<V1.PlayerJoined>(HandlePlayerJoined);
    On<V1.DiceRolled>(HandleDiceRolled);
    On<V2.DiceRolled>(HandleDiceRolledV2);
    On<V1.TurnPassed>(HandleTurnPassed);
    On<V1.DiceKept>(HandleDiceKept);
    On<V2.DiceKept>(HandleDiceKeptV2);
  }

  public GameId    Id        { get; private init; }
  public GameStage GameStage { get; private init; }
  public Score     TurnScore { get; private init; } = new(0);

  public ImmutableArray<Player>    Players     { get; private init; } = ImmutableArray<Player>.Empty;
  public ImmutableArray<DiceValue> TableCenter { get; private init; } = ImmutableArray<DiceValue>.Empty;
  public ImmutableArray<DiceValue> DiceKept    { get; private init; } = ImmutableArray<DiceValue>.Empty;

  public ImmutableDictionary<int, int> ScoreTable { get; private init; } =
    ImmutableDictionary<int, int>.Empty;

  internal int PlayerInTurn => Players[0].Id;

  public Score  GameScoreFor(PlayerId playerId) => new(ScoreTable[playerId]);
  public Player GetPlayer(int         id)       => Players.Single(p => p.Id == id);

  private static GameState HandleDiceKept(GameState state, V1.DiceKept e)
    => state with
    {
      DiceKept = state.DiceKept.AddRange(Dice.FromValues(e.Dice).DiceValues),
      TurnScore = e.NewTurnScore,
      TableCenter = ImmutableArray<DiceValue>.Empty.AddRange(e.TableCenter.ToDiceValues()),
      GameStage = GameStage.Rolling
    };

  private static GameState HandleDiceKeptV2(GameState state, V2.DiceKept e)
    => state with
    {
      DiceKept = state.DiceKept.AddRange(Dice.FromValues(e.Dice).DiceValues),
      TurnScore = e.NewTurnScore,
      TableCenter = ImmutableArray<DiceValue>.Empty.AddRange(e.TableCenter.ToDiceValues()),
      GameStage = e.Stage
    };

  private static GameState HandleTurnPassed(GameState state, V1.TurnPassed e)
    => state with
    {
      Players = e.PlayerOrder,
      ScoreTable = state.ScoreTable.SetItem(
        e.PlayerId,
        e.GameScore),
      TableCenter = state.TableCenter.AddRange(state.DiceKept),
      GameStage = GameStage.Rolling,
      DiceKept = state.TableCenter.Clear()
    };

  private static GameState HandleDiceRolled(GameState state, V1.DiceRolled e)
    => state with
    {
      TurnScore = e.TurnScore,
      TableCenter = ImmutableArray<DiceValue>.Empty.AddRange(e.Dice.ToDiceValues()),
      GameStage = GameStage.Keeping
    };

  private static GameState HandleDiceRolledV2(GameState state, V2.DiceRolled e)
    => state with
    {
      TurnScore = e.TurnScore,
      TableCenter = ImmutableArray<DiceValue>.Empty.AddRange(e.Dice.ToDiceValues()),
      GameStage = e.Stage
    };

  private static GameState HandlePlayerJoined(GameState state, V1.PlayerJoined playerJoined)
    => state with
    {
      Players = state.Players.Add(new Player(playerJoined.Id, playerJoined.Name)),
      ScoreTable = state.ScoreTable.Add(playerJoined.Id, 0)
    };

  private static GameState HandleGameStarted(GameState gameState, V1.GameStarted e)
    => gameState with
    {
      Id = e.Id,
      GameStage = GameStage.Rolling
    };
}

public record Score(int Value) {
  public static implicit operator int(Score score) => score.Value;

  public static implicit operator Score(int score) => new(score);
}

public record GameId(int Id) : AggregateId($"{Id}") {
  public static implicit operator GameId(int id) => new(id);
  public static implicit operator int(GameId id) => id.Id;
}