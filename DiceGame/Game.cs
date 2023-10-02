using System.Collections.Immutable;
using static DiceGame.GameEvents;
using static DiceGame.GameStage;
using static DiceGame.GameValidator;

namespace DiceGame;

public class Game {
  private readonly List<object> _events = new();
  private IRandom randomProvider;

  public Game(IRandom randomProvider = null) {
    this.randomProvider = randomProvider ?? new DefaultRandomProvider();
  }

  public GameState State { get; private set; } =
    new GameState(0, None, ImmutableArray<Player>.Empty, 0);

  public void Start(int gameId) => Apply(new GameStarted(gameId));

  private void Apply(object @event) {
    try
    {
      EnsurePreconditions(this.State, @event);
    }
    catch (PreconditionsFailedException e)
    {
      _events.Add(e.Event);
      throw;
    }

    State = State.When(@event);
    _events.Add(@event);
  }

  public IReadOnlyList<object> Events => _events.AsReadOnly();

  public void Load(IEnumerable<GameStarted> events) {
    foreach (var @event in events)
    {
      State = State.When(@event);
    }
  }

  public void JoinPlayer(int id, string name) {
    Apply(new PlayerJoined(id, name));
  }

  public void ThrowDice(int id) {
    var @throw = Dice.FromNewThrow(
      randomProvider,
      IsFirstThrow() ? 6 : 
      State.TableCenter.Count());
    Apply(new DiceThrown(id, @throw.DiceValues.Select(d => (int)d).ToArray()
    ));
  }

  private bool IsFirstThrow() =>
    !State.Throws.Any();

  public void Pass(int id) => Apply(new TurnPassed(id, RotatePlayer(id)));

  private ImmutableArray<Player> RotatePlayer(int playerId) {
    var player = State.GetPlayer(playerId);
    var newPlayerList = State.Players.Remove(player).Add(player);
    return newPlayerList;
  }

  public void Keep(int playerId, DiceValue[] diceValues) {
    Apply(new DiceKept(playerId, diceValues.Select(d => (int)d).ToArray(), RotatePlayer(playerId)));
  }
}

public class DefaultRandomProvider : IRandom {
  public int Next(int minValue, int maxValue) =>
    (new Random()).Next(minValue, maxValue);
}

public record DiceKept(int PlayerId, int[] Dice, ImmutableArray<Player> immutableArray);

public record TurnPassed(int PlayerId, ImmutableArray<Player> RotatedPlayers);

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Started
}