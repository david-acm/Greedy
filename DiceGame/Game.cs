using System.Collections.Immutable;

namespace DiceGame;

public class Game {
  private List<object> _events = new();

  public GameState State { get; private set; } =
    new GameState(0, GameStage.None, ImmutableArray<Player>.Empty);

  public void Start(int gameId) => Apply(new GameEvents.GameStarted(gameId));

  private void Apply(object @event) {
    EnsurePreconditions(@event);
    State = State.When(@event);
    _events.Add(@event);
  }

  private void EnsurePreconditions(object @event) {
    var valid = @event switch
    {
      DiceGame.GameEvents.GameStarted => State.GameStage == GameStage.None,
      DiceGame.GameEvents.PlayerJoined => State.GameStage == GameStage.Started,
      _ => true
    };

    if (!valid) throw new PreconditionsFailedException();
  }

  public IReadOnlyList<object> Events => _events.AsReadOnly();

  public void Load(GameEvents.GameStarted[] events) {
    foreach (var @event in events)
    {
      State = State.When(@event);
    }
  }

  public void JoinPlayer(int id, string name) {
    Apply(new GameEvents.PlayerJoined(id, name));
  }

  public void ThrowDice(int id) {
    var @throw = Dice.FromNewThrow();
    Apply(new GameEvents.DiceThrown(id, 
      (int)@throw.DiceOne,
      (int)@throw.DiceTwo,
      (int)@throw.DiceThree,
      (int)@throw.DiceFour,
      (int)@throw.DiceFive,
      (int)@throw.DiceSix
      ));
  }
}

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Started
}

public class PreconditionsFailedException : Exception {
  public PreconditionsFailedException() {
  }
}