using System.Collections.Immutable;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.GameAggregate;

public record GameState(
  int                    Id,
  GameStage              GameStage,
  ImmutableArray<Player> Players,
  int                    PlayerInTurn
) {
  public Play? LastThrow => Throws.LastOrDefault();

  public ImmutableArray<Play> Throws { get; private set; } = ImmutableArray<Play>.Empty;

  public   ImmutableArray<DiceValue> DiceKept     { get; private set; } = ImmutableArray<DiceValue>.Empty;
  internal int                       PlayerInTurn => Players[0].Id;

  public IEnumerable<DiceValue> TableCenter => LastThrow is null
    ? ImmutableArray<DiceValue>.Empty
    : ImmutableArray<DiceValue>.Empty.AddRange(LastThrow.Dice.DiceValues).RemoveRange(DiceKept);

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
      _scoreTable = _scoreTable.SetItem(diceKept.PlayerId, GetScore(diceKept))
    };

  private GameState HandleTurnPassed(GameState state, TurnPassed e)
    => state with
    {
      Players = e.RotatedPlayers,
    };

  private static int GetScore(DiceKept diceKept) {
    var dice = Dice.FromValues(diceKept.Dice);
    var tricks = new Dictionary<Validator, int>
    {
      { new DiceAreTrips(dice), dice.DiceValues.First().Value                             * 100 },
      { new DiceAreOnesOrFives(dice), dice.DiceValues.Count(d => d == DiceValue.One) * 100 + dice.DiceValues.Count(d => d == DiceValue.Five) * 50 },
      { new DiceAreStair(dice), 1500 }
    };

    return tricks.First(v => v.Key.IsSatisfied()).Value;
  }

  private GameState HandleDiceThrown(GameState gameState, DiceThrown diceThrown)
    => gameState with
    {
      Throws = Throws.Add(new Play(
        diceThrown.PlayerId,
        new Dice(diceThrown.Dice.ToDiceValues()))),
      _scoreTable = _scoreTable.SetItem(diceThrown.PlayerId, new CanKeepDice(new Dice(diceThrown.Dice.ToDiceValues())).IsSatisfied() ? _scoreTable[diceThrown.PlayerId] : 0)
    };

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

  public  Score                              TurnScoreFor(PlayerId playerId) => new Score(playerId, _scoreTable[playerId]);
  private ImmutableDictionary<PlayerId, int> _scoreTable = ImmutableDictionary<PlayerId, int>.Empty;
}

public record Score(PlayerId PlayerId, int Value);

public record Play(int PlayerId, Dice Dice);