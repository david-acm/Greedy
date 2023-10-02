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

  public GameState State { get; private set; } = new(0, None, ImmutableArray<Player>.Empty, 0);

  public IReadOnlyList<object> Events => _events.AsReadOnly();

  public void Start(StartGame startGame) => Apply(new GameStarted(startGame));

  public void JoinPlayer(JoinPlayer joinPlayer) =>
    Apply(new PlayerJoined(joinPlayer.Id, joinPlayer.Name));

  public void ThrowDice(PlayerId playerId) {
    var @throw = Dice.FromNewThrow(
      _randomProvider,
      GetNumberOfDiceToTrow());
    Apply(new DiceThrown(playerId, @throw.DiceValues.ToPrimitiveArray()));
  }

  public void Pass(PlayerId id) => Apply(new TurnPassed(id, RotatePlayer(id)));

  public void Load(IEnumerable<object> events) {
    foreach (var @event in events) State = State.When(@event);
  }

  private ImmutableArray<Player> RotatePlayer(int playerId) {
    var player        = State.GetPlayer(playerId);
    var newPlayerList = State.Players.Remove(player).Add(player);
    return newPlayerList;
  }

  public void Keep(int playerId, IEnumerable<DiceValue> diceValues) =>
    Apply(new DiceKept(playerId, diceValues.ToPrimitiveArray()));

  private int GetNumberOfDiceToTrow() =>
    State.IsFirstThrow ? 6 : State.TableCenter.Count();

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
}

public class DefaultRandomProvider : IRandom {
  public int Next(int minValue, int maxValue) =>
    new Random().Next(minValue, maxValue);
}

public record DiceKept(int PlayerId, int[] Dice);

public record TurnPassed(int PlayerId, ImmutableArray<Player> RotatedPlayers);

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Started
}