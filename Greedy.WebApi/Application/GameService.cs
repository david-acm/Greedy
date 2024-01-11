using Eventuous;
using Greedy.GameAggregate;
using static Eventuous.ExpectedState;

namespace Greedy.WebApi.Application;

public class GameService : CommandService<Game, GameState, GameId> {
  public GameService(IAggregateStore store, IConfiguration config) : base(store)
  {
    On<Command.StartGame>()
      .InState(New)
      .GetId(cmd => new GameId(cmd.GameId))
      .Execute((game, cmd) => game.Start(cmd));
    
    // Controlador
    // [Authorize(Player)]
    // public async Task<IHttpResponse> Game(StartGame dto) { // On<Command.StartGame>()
    //   var id = new GameId(dto.GameId) // GetId(cmd => new GameId(cmd.GameId))
    //   var game = new Game(id); // CommandService<Game
    //   game.Start(dto) // .Execute((game, cmd) => game.Start(cmd))
    // }

    On<Command.JoinPlayer>().InState(Existing).GetId(cmd => new GameId(cmd.GameId)).
      Execute((game, cmd) => game.JoinPlayer(cmd));

    On<Command.RollDice>().InState(Existing).GetId(cmd => new GameId(cmd.GameId)).
      Execute((game, cmd) => game.RollDiceV2(cmd));

    On<Command.KeepDice>().InState(Existing).GetId(cmd => new GameId(cmd.GameId)).
      Execute((game, cmd) => game.KeepDice(cmd));

    On<Command.PassTurn>().InState(Existing).GetId(cmd => new GameId(cmd.GameId)).
      Execute((game, cmd) => game.PassTurn(cmd));
  }
}