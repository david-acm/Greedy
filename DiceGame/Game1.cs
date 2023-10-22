namespace DiceGame;
/*
public class Game1 {
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
*/

public class Game1 {
  public List<object> Events { get; private set; } = new();

  public void Start(int gameId) {
    // 1
    Events.Add(new GameStarted(gameId));
    Stage = GameStage.Started;
  }

  // 1. Ver la transicion de estados: None, Started, Paused, Finished
  // 2. Recuperar el estado actual a partir de los eventos anteriores. Solo asi se garantiza que el log de eventos es acertado en todos los momentos

  // sourcing origen. 

  public GameStage Stage { get; set; }

  public void JoinPlayer(string playerName) {
      
      
  }
}

public class GameService1 {
  private readonly IEventStore _eventStore;

  public GameService1(IEventStore eventStore) {
    _eventStore = eventStore;
  }

  public void StartGame(int id) {
    var game = new Game1();
    game.Start(id);
    _eventStore.Save(game.Events.ToArray());
  }

  public Game1 JoinPlayer(int id, string playerName) {
    var gameEvents = _eventStore.Load<Game1>(id);

    var game = new Game1();
    // game.Load(gameEvents);
    game.JoinPlayer(playerName);
    
    return game;
  }
}

