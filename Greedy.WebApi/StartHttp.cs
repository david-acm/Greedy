using Eventuous.AspNetCore.Web;
using Greedy.GameAggregate;

namespace Greedy.WebApi;

[AggregateCommands<Game>]
public static class V1 {
  [HttpCommand(Route = "games")]
  public record StartHttp(int Id);
  
  [HttpCommand(Route = "players")]
  public record Join(int Id, int PlayerId, string PlayerName);
}