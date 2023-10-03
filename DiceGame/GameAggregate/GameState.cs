using System.Collections.Immutable;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.GameAggregate;

public record GameState(
  int                    Id,
  GameStage              GameStage,
  ImmutableArray<Player> Players
) {
  public Play? LastRoll => Rolls.LastOrDefault();

  public ImmutableArray<Play> Rolls { get; private set; } = ImmutableArray<Play>.Empty;

  public   ImmutableArray<DiceValue> DiceKept     { get; private set; } = ImmutableArray<DiceValue>.Empty;
  internal int                       PlayerInTurn => Players[0].Id;

  public IEnumerable<DiceValue> TableCenter
  {
    get
    {
      var @Roll = ImmutableArray<DiceValue>.Empty.AddRange(LastRoll.Dice.DiceValues);
      DiceKept.ToList().ForEach(k => @Roll = @Roll.Remove(k));
      return LastRoll is null
        ? ImmutableArray<DiceValue>.Empty
        : @Roll;
    }
  }

  public bool IsFirstRoll => LastRoll is null;

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

  private GameState HandleDiceKept(GameState state, DiceKept diceKept) =>
    state with
    {
      DiceKept = DiceKept.AddRange(Dice.FromValues(diceKept.Dice).DiceValues),
      TurnScore = diceKept.newTurnScore
    };

  private GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with
    {
      Players = e.RotatedPlayers,
      _scoreTable = _scoreTable.SetItem(
        PlayerInTurn, 
        _scoreTable[PlayerInTurn] + TurnScore)
    };


  private GameState HandleDiceRolled(GameState gameState, DiceRolled diceRolled)
    => gameState with
    {
      Rolls = Rolls.Add(new Play(
        diceRolled.PlayerId,
        new Dice(diceRolled.Dice.ToDiceValues()))),
      TurnScore = ResetScoreWhenGreedy(diceRolled)
    };

  private Score ResetScoreWhenGreedy(DiceRolled diceRolled) {
    var canKeepAnyDice = new CanKeepDice(new Dice(diceRolled.Dice.ToDiceValues())).IsSatisfied();

    return canKeepAnyDice ? TurnScore : new Score(0);
  }

  public Player GetPlayer(int id) => Players.Single(p => p.Id == id);


  private GameState HandlePlayerJoined(GameState gameState, PlayerJoined playerJoined)
    => gameState with
    {
      Players = Players.Add(new Player(playerJoined.Id, playerJoined.Name)),
      _scoreTable = _scoreTable.Add(playerJoined.Id, 0)
    };

  private GameState HandleGameStarted(
    GameState   gameState,
    GameStarted e
  ) => gameState with
  {
    Id = e.Id,
    GameStage = GameStage.Started
  };

  public Score TurnScore { get; private set; } = new(0);

  private ImmutableDictionary<PlayerId, int> _scoreTable         = ImmutableDictionary<PlayerId, int>.Empty;

  public Score GameScoreFor(PlayerId playerId) => new(_scoreTable[playerId]);
}

public record Score(int Value) {
  public static implicit operator int(Score score) => score.Value;

  public static implicit operator Score(int score) => new Score(score);
}

public record Play(int PlayerId, Dice Dice);