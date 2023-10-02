namespace DiceGame;

public class PreconditionsFailedException : Exception {
  public object Event { get; }

  public PreconditionsFailedException(string reason, object @event) : base(reason) {
    Event = @event;
  }
}