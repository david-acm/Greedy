namespace DiceGame;

public class Game {
  private List<object> _events = new();

  public void Start(int gameId) => Apply(new GameStarted(gameId));

  private void Apply(object @event) {
    EnsurePreconditions(@event);
    When(@event);
    this._events.Add(@event);
  }

  private void EnsurePreconditions(object @event) {
    var valid = @event switch
    {
      GameStarted e => this.State == GameState.None
    };

    if (!valid) throw new PreconditionsFailedException();
  }

  public GameState State { get; private set; }
  public IReadOnlyList<object> Events => _events.AsReadOnly();
  public int Id { get; private set; }

  public void Load(GameStarted[] events) {
    foreach (var @event in events)
    {
      When(@event);
    }
  }

  private void When(object @event) {
    var action = @event switch
    {
      GameStarted e => (Action)(() =>
      {
        Id = e.Id;
        State = GameState.Started;
      })
    };
    action();
  }
}

public enum GameState {
  None,
  Started
}

public record GameStarted(int Id);

public class PreconditionsFailedException : Exception {
  public PreconditionsFailedException() {
    
  }
}