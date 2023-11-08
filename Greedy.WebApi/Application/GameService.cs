using Eventuous;
using Greedy.GameAggregate;
using static Eventuous.CommandServiceDelegates;
using static Eventuous.ExpectedState;

namespace Greedy.WebApi.Application;

public class GameService : CommandService<Game, GameState, GameId> {
  public GameService(IAggregateStore store) : base(store) {
    On<Command.StartGame>()
      .InState(New)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Execute((game, cmd) => game.Start(cmd));

    On<Command.JoinPlayer>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Execute((game, cmd) => game.JoinPlayer(cmd));

    On<Command.RollDice>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Execute((game, cmd) => game.RollDice(cmd));

    On<Command.KeepDice>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Execute((game, cmd) => game.KeepDice(cmd));

    On<Command.PassTurn>()
      .InState(Existing)
      .GetId((cmd) => new GameId(cmd.GameId))
      .Execute((game, cmd) => game.PassTurn(cmd));
  }
}

public static class CommandHandlerBuilderExtensions {
  public static CommandHandlerBuilder<TCommand, TAggregate, TState, TId> Execute<TCommand, TAggregate, TState, TId>(
    this CommandHandlerBuilder<TCommand, TAggregate, TState, TId> builder, ActOnAggregate<TAggregate, TCommand> action)
    where TCommand : class
    where TAggregate : Aggregate<TState>, new()
    where TState : State<TState>, new()
    where TId : Id {
    builder.Act((game, cmd) =>
    {
      try
      {
        action.Invoke(game, cmd);
      }
      catch (DomainException e)
      {
        // We ignore the domain exceptions because other wise the error events would not be persisted to the store. In a future version these events will be handled and will return the appropriate HTTP 400 Bad Request response
      }
    });

    return builder;
  }
}