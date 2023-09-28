namespace DiceGame;

public class Game {
  private List<object> _events = new();

  public void Start(int gameId) => Apply(new GameStarted(gameId));

  private void Apply(object @event) {
    When(@event);
    this._events.Add(@event);
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