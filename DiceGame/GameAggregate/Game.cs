using System.Collections.Immutable;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.GameEvents;
using static DiceGame.GameAggregate.GameStage;
using static DiceGame.GameAggregate.GameValidator;

namespace DiceGame.GameAggregate;

public class Game {
  private readonly List<object> _events = new();
  private readonly IRandom      _randomProvider;

  public Game(IRandom randomProvider = null!) {
    _randomProvider = randomProvider ?? new DefaultRandomProvider();
  }

  public GameState State { get; private set; } = new(0, None, ImmutableArray<Player>.Empty);

  public IReadOnlyList<object> Events => _events.AsReadOnly();

  public void Start(StartGame startGame) => Apply(new GameStarted(startGame));

  public void JoinPlayer(JoinPlayer joinPlayer) =>
    Apply(new PlayerJoined(joinPlayer.Id, joinPlayer.Name));

  public void RollDice(PlayerId playerId) {
    var @Roll = Dice.FromNewRoll(
      _randomProvider,
      GetNumberOfDiceToTrow());
    Apply(new DiceRolled(playerId, @Roll.DiceValues.ToPrimitiveArray()));
  }

  public void Pass(PlayerId playerId) => Apply(new TurnPassed(playerId, RotatePlayer(playerId)));

  public void Load(IEnumerable<object> events) {
    foreach (var @event in events) State = State.When(@event);
  }

  private ImmutableArray<Player> RotatePlayer(int playerId) {
    var player        = State.GetPlayer(playerId);
    var newPlayerList = State.Players.Remove(player).Add(player);
    return newPlayerList;
  }

  public void Keep(Keep keep) =>
    Apply(new DiceKept(keep.PlayerId, keep.DiceValues.ToPrimitiveArray(),
      GetNewTurnScore(keep.DiceValues, State.TurnScore)));

  private int GetNumberOfDiceToTrow() =>
    State.IsFirstRoll ? 6 : 6 - State.DiceKept.Length;

  private void Apply(object @event) {
    try
    {
      EnsurePreconditions(State, @event);
    }
    catch (PreconditionsFailedException e)
    {
      _events.Add(e.Event);
      throw;
    }

    State = State.When(@event);
    _events.Add(@event);
  }
  
  
  private static int GetNewTurnScore(IEnumerable<DiceValue> diceKept, int currentScore) {
    var dice = new Dice(diceKept);
    var tricks = new Dictionary<Validator, int>
    {
      { new DiceAreTrips(dice), dice.DiceValues.First().Value * 100 },
      {
        new DiceAreOnesOrFives(dice),
        dice.DiceValues.Count(d => d == DiceValue.One) * 100 + dice.DiceValues.Count(d => d == DiceValue.Five) * 50
      },
      { new DiceAreStair(dice), 1500 }
    };

    var turnScore = tricks.FirstOrDefault(v => v.Key.IsSatisfied()).Value;

    return new Score(currentScore + turnScore);
  }
}

public record DiceKept(int PlayerId, int[] Dice, int newTurnScore);

public record TurnPassed(int PlayerId, ImmutableArray<Player> RotatedPlayers);

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Started
}