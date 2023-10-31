namespace DiceGame;

public class GameService {
  private readonly IEventStore _store;

  public GameService(IEventStore store) {
    _store = store;
  }
  
  public void StartGame(int id) {
    var game = new Game();
    game.Start(id);
    
    _store.Save(game.Events.ToArray());
  }

  public Game JoinGame(int gameId,string name) {
    // var gameEvents = _store.Load<Game>(gameId);
    var game = new Game();

    // game.Load(gameEvents);
    
    game.JoinPlayer(name);
    
    return game;
  }
}