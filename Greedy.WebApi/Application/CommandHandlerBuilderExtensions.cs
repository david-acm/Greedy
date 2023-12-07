using Eventuous;

namespace Greedy.WebApi.Application;

public static class CommandHandlerBuilderExtensions {
  public static CommandHandlerBuilder<TCommand, TAggregate, TState, TId> Execute<TCommand, TAggregate, TState, TId>(
    this CommandHandlerBuilder<TCommand, TAggregate, TState, TId> builder, CommandServiceDelegates.ActOnAggregate<TAggregate, TCommand> action)
    where TCommand : class
    where TAggregate : Aggregate<TState>, new()
    where TState : State<TState>, new()
    where TId : Id
  {
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