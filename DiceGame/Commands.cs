namespace DiceGame;

public static class Commands {

  public record GameId(int Id) {
    public static implicit operator GameId(int id) => new GameId(id);
  }
  public record StartGame(GameId Id) {
    public static implicit operator int(StartGame startGame) => startGame.Id.Id;
  }

  public record JoinPlayer(PlayerId Id, string Name) {
  }

  public record PlayerId(int Id) {
    public static implicit operator int(PlayerId id) => id.Id;
    public static implicit operator PlayerId(int id) => new PlayerId(id);
  }
}