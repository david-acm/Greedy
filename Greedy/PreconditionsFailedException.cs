namespace Greedy;

public class PreconditionsFailedException : Exception {
  public PreconditionsFailedException(string reason, object @event) : base(reason) {
    Event = @event;
  }

  public object Event { get; }
}