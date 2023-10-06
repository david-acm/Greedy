using Eventuous;
using Greedy.GameAggregate;
using Microsoft.AspNetCore.Mvc;
using static Eventuous.ExpectedState;

namespace Greedy.WebApi.Application;

public class GameService : CommandService<Game, GameState, GameId> {
  public GameService(IAggregateStore store) : base(store) {
    On<Command.StartGame>()
      .InState(New)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Act((game, cmd) => game.Start(cmd));

    On<Command.JoinPlayer>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Act((game, cmd) => game.JoinPlayer(cmd));
    
    On<Command.RollDice>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Act((game, cmd) => game.RollDice(cmd));
    
    On<Command.KeepDice>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Act((game, cmd) => game.KeepDice(cmd));
    
    
    On<Command.PassTurn>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Act((game, cmd) => game.PassTurn(cmd));
  }
}