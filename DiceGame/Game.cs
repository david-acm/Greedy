using System.Collections.Immutable;
using static DiceGame.GameEvents;
using static DiceGame.GameStage;

namespace DiceGame;

public class Game {
  private List<object> _events = new();

  public GameState State { get; private set; } =
    new GameState(0, None, ImmutableArray<Player>.Empty, 0);

  public void Start(int gameId) => Apply(new GameStarted(gameId));

  private void Apply(object @event) {
    EnsurePreconditions(@event);
    State = State.When(@event);
    _events.Add(@event);
  }

  private void EnsurePreconditions(object @event) {
    var valid = @event switch
    {
      GameStarted e => Validate(
        State.GameStage == None, new GameAlreadyStarted()),
      PlayerJoined => Validate(
        State.GameStage == Started, new GameHasNotStarted(State.GameStage)),
      DiceThrown e => Validate(
        State.PlayerInTurn == e.PlayerId, new PlayedOutOfTurn(e.PlayerId, State.PlayerInTurn)),
      
      _ => Validate(false, $"No validation performed for event {@event}")
    };


    if (valid) return;
    
    _events.Add(valid.FailedValidationEvent);
    throw new PreconditionsFailedException(valid.FailedValidationEvent.ToString()!);
  }

  private static ValidationResult Validate(bool validation, object failedValidationEvent) {
    return new ValidationResult(validation, failedValidationEvent);
  }

  public IReadOnlyList<object> Events => _events.AsReadOnly();

  public void Load(GameStarted[] events) {
    foreach (var @event in events)
    {
      State = State.When(@event);
    }
  }

  public void JoinPlayer(int id, string name) {
    Apply(new PlayerJoined(id, name));
  }

  public void ThrowDice(int id) {
    var @throw = Dice.FromNewThrow();
    Apply(new DiceThrown(id, 
      (int)@throw.DiceOne,
      (int)@throw.DiceTwo,
      (int)@throw.DiceThree,
      (int)@throw.DiceFour,
      (int)@throw.DiceFive,
      (int)@throw.DiceSix
      ));
  }
}

internal record GameAlreadyStarted;

internal record GameHasNotStarted(GameStage GameStage);

internal record ValidationResult(bool IsValid, object  FailedValidationEvent) {
  public static implicit operator bool(ValidationResult result) => result.IsValid;
}

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Started
}

public class PreconditionsFailedException : Exception {
  public PreconditionsFailedException(string reason) : base(reason) {
  }
}