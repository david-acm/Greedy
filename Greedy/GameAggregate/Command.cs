namespace Greedy.GameAggregate;


public static class Command {
  public record KeepDice(GameId GameId, PlayerId PlayerId, IEnumerable<DiceValue> DiceValues);

  public record StartGame(GameId GameId) {
    public static implicit operator int(StartGame startGame) => startGame.GameId.Id;
  }

  public record JoinPlayer(int GameId, PlayerId Id, string Name);

  public record RollDice(GameId GameId, PlayerId PlayerId);
  public record PassTurn(GameId GameId, PlayerId PlayerId);

  public record PlayerId(int Id) {
    public static implicit operator int(PlayerId id) => id.Id;
    public static implicit operator PlayerId(int id) => new(id);
  }
}