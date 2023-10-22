namespace DiceGame;

public class Game {
  public List<object> Events { get; set; } = new();
  public void Start(int id) {
    // Event GameStarted
    var gameStarted = new GameStarted(id);
    Events.Add(gameStarted);
    Stage = GameStage.Started;
  }

  public void JoinPlayer(string name) {
    
    var playerJoined = new PlayerJoined(name);
    Events.Add(playerJoined);
    Players.Add(new Player(name));
  }
  
  public GameStage    Stage   { get; set; }
  public List<Player> Players { get; set; } = new();

  public void Load(object[] gameEvents) {

    foreach (var @event in gameEvents)
    {
      var action = @event switch
      {
        GameStarted e => (Action)(() =>
        {
          Id    = e.GameId;
          Stage = GameStage.Started;
        })
      };
      action();
    }
  }

  public int Id { get; set; }
}


public record Player(string Name);

public enum GameStage {
  None,
  Started
}