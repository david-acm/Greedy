using Eventuous.AspNetCore.Web;
using Greedy.GameAggregate;

namespace Greedy.WebApi;

[AggregateCommands<Game>]
public static class V1 {
  [HttpCommand(Route = "games")]
  public record StartGameHttp(int Id);
  
  [HttpCommand(Route = "players")]
  public record JoinPlayerHttp(int GameId, int PlayerId, string PlayerName);
  
  [HttpCommand(Route = "diceRolls")]
  public record RollDiceHttp(int GameId, int PlayerId);
  
  [HttpCommand(Route = "diceKeeps")]
  public record KeepDiceHttp(int GameId, int PlayerId, int[] DiceValues);
  
  [HttpCommand(Route = "turnPasses")]
  public record PassTurnHttp(int GameId, int PlayerId);
}