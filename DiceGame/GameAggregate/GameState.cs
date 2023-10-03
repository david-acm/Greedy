using System.Collections.Immutable;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.GameAggregate;

public record GameState(
  int                    Id,
  GameStage              GameStage,
  ImmutableArray<Player> Players
) {
  public Play? LastThrow => Throws.LastOrDefault();

  public ImmutableArray<Play> Throws { get; private set; } = ImmutableArray<Play>.Empty;

  public   ImmutableArray<DiceValue> DiceKept     { get; private set; } = ImmutableArray<DiceValue>.Empty;
  internal int                       PlayerInTurn => Players[0].Id;

  public IEnumerable<DiceValue> TableCenter
  {
    get
    {
      var @throw = ImmutableArray<DiceValue>.Empty.AddRange(LastThrow.Dice.DiceValues);
      DiceKept.ToList().ForEach(k => @throw = @throw.Remove(k));
      return LastThrow is null
        ? ImmutableArray<DiceValue>.Empty
        : @throw;
    }
  }

  public bool IsFirstThrow => LastThrow is null;

  public GameState When(object @event) {
    var state = @event switch
    {
      GameStarted e  => HandleGameStarted(this, e),
      PlayerJoined e => HandlePlayerJoined(this, e),
      DiceThrown e   => HandleDiceThrown(this, e),
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
      TurnScore = GetScore(diceKept, TurnScore)
    };

  private GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with
    {
      Players = e.RotatedPlayers,
      _scoreTable = _scoreTable.SetItem(
        PlayerInTurn, 
        _scoreTable[PlayerInTurn] + TurnScore)
    };

  private static Score GetScore(DiceKept diceKept, int currentScore) {
    var dice = Dice.FromValues(diceKept.Dice);
    var tricks = new Dictionary<Validator, int>
    {
      { new DiceAreTrips(dice), dice.DiceValues.First().Value * 100 },
      {
        new DiceAreOnesOrFives(dice),
        dice.DiceValues.Count(d => d == DiceValue.One) * 100 + dice.DiceValues.Count(d => d == DiceValue.Five) * 50
      },
      { new DiceAreStair(dice), 1500 }
    };

    var turnScore = tricks.First(v => v.Key.IsSatisfied()).Value;

    return new Score(currentScore + turnScore);
  }

  private GameState HandleDiceThrown(GameState gameState, DiceThrown diceThrown)
    => gameState with
    {
      Throws = Throws.Add(new Play(
        diceThrown.PlayerId,
        new Dice(diceThrown.Dice.ToDiceValues()))),
      TurnScore = ResetScoreWhenGreedy(diceThrown)
    };

  private Score ResetScoreWhenGreedy(DiceThrown diceThrown) {
    var canKeepAnyDice = new CanKeepDice(new Dice(diceThrown.Dice.ToDiceValues())).IsSatisfied();

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
}

public record Play(int PlayerId, Dice Dice);