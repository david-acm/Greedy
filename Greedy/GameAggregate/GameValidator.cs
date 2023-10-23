using Ardalis.GuardClauses;
using Eventuous;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.GameAggregate;

public static class GameValidator {
  public static void EnsurePreconditions(Game game, object @event) {
    var state = game.State;
    var valid = @event switch
    {
      V1.GameStarted e => Validate(
        state.GameStage == GameStage.None,
        new GameAlreadyStarted(e.Id)),
      V1.PlayerJoined => Validate(
        state.GameStage == GameStage.Rolling,
        new GameHasNotStarted(state.GameStage)),
      V1.DiceRolled e =>
        new PlayerIsInTurn(state, e.PlayerId)
          .And(new SingleRoll(state, e.PlayerId))
          .IsSatisfied(),
      V2.DiceRolled e =>
        new PlayerIsInTurn(state, e.PlayerId)
          .And(new SingleRoll(state, e.PlayerId))
          .IsSatisfied(),
      V1.TurnPassed e =>
        new PlayerIsInTurn(state, e.PlayerId)
          .And(new PlayerCanPass(game, e.PlayerId))
          .IsSatisfied(),
      V2.DiceKept e =>
        new PlayerIsInTurn(state, e.PlayerId)
          .And(new PlayerHasThoseDice(GetDice(e), state))
          .And(new CanKeepDice(GetDice(e)))
          .IsSatisfied(),

      _ => Validate(false, $"No validation performed for event {@event}")
    };

    if (valid) return;

    throw new PreconditionsFailedException(valid.FailedValidationEvent.ToString()!, valid.FailedValidationEvent);
  }

  private static Dice GetDice(V2.DiceKept e) =>
    Dice.FromValues(e.Dice.ToList());

  private static ValidationResult Validate(bool validation, object failedValidationEvent) =>
    new(validation, failedValidationEvent);
}

public class SingleRoll : Validator {
  private readonly GameState _state;
  private readonly int       _playerId;

  public SingleRoll(GameState state, int playerId) {
    _state    = state;
    _playerId = playerId;
  }

  public override ValidationResult IsSatisfied()
    => new(_state.GameStage == GameStage.Rolling,
      new V1.RolledTwice(_playerId));
}

public class PlayerHasThoseDice : Validator {
  private readonly Dice      _dice;
  private readonly GameState _state;

  public PlayerHasThoseDice(Dice dice, GameState state) {
    _dice  = dice;
    _state = state;
  }

  public override ValidationResult IsSatisfied() {
    var tableCenter = _state.TableCenter.ToList();
    var unavailable = _dice.DiceValues.Where(d => !tableCenter.Remove(d)).ToList();
    return new ValidationResult(
      !unavailable.Any(),
      new DiceNotAllowedToBeKept(
        $"Player Does not have die/dice: {string.Join(',', unavailable)}. Dice found: {string.Join(", ", _state.TableCenter)}",
        unavailable.ToPrimitiveArray()));
  }
}

public class DiceAreStair : Validator {
  private readonly IEnumerable<DiceValue> _dice;

  public DiceAreStair(Dice dice) {
    _dice = dice.DiceValues;
  }

  public override ValidationResult IsSatisfied() =>
    new(_dice.Count() == 6              &&
        _dice.Contains(DiceValue.One)   &&
        _dice.Contains(DiceValue.Two)   &&
        _dice.Contains(DiceValue.Three) &&
        _dice.Contains(DiceValue.Four)  &&
        _dice.Contains(DiceValue.Five)  &&
        _dice.Contains(DiceValue.Six),
      new DiceNotAllowedToBeKept("Dice are not a stair", _dice.ToPrimitiveArray())
    );
}

[EventType("V1.GameAlreadyStarted")]
internal record GameAlreadyStarted(int Id);

[EventType("V1.GameHasNotStarted")]
internal record GameHasNotStarted(GameStage GameStage);

[EventType("V1.DiceNotAllowedToBeKept")]
public record DiceNotAllowedToBeKept(string Reason, IEnumerable<int> Dice);

public class DiceAreOnesOrFives : Validator {
  private readonly IEnumerable<DiceValue> _dice;

  public DiceAreOnesOrFives(Dice dice) {
    _dice = dice.DiceValues;
  }

  public override ValidationResult IsSatisfied() =>
    new(_dice.All(d => d == DiceValue.One || d == DiceValue.Five),
      new DiceNotAllowedToBeKept("Dice are not ones or fives", _dice.ToPrimitiveArray()));
}

public class CanKeepDice : Validator {
  private readonly IEnumerable<DiceValue> _dice;

  public CanKeepDice(Dice dice) {
    _dice = dice.DiceValues;
  }

  public override ValidationResult IsSatisfied() {
    var diceContainOnesOrFives          = _dice.Any(d => d == DiceValue.One || d == DiceValue.Five);
    var thereAreThreeOrMoreRepeatedDice = _dice.GroupBy(d => d).MaxBy(d => d.Count())?.Count() >= 3;

    return new(
      diceContainOnesOrFives || thereAreThreeOrMoreRepeatedDice,
      new DiceNotAllowedToBeKept("Dice are not ones or fives",
        _dice.ToPrimitiveArray()));
  }
}

public class DiceAreTrips : Validator {
  private readonly IEnumerable<DiceValue> _dice;

  public DiceAreTrips(Dice dice) {
    _dice = dice.DiceValues;
  }

  public override ValidationResult IsSatisfied() =>
    new(AreThree(_dice) && AllDiceHaveTheSameValue(_dice), $"The dice {_dice} are not trips.");

  private static bool AreThree(IEnumerable<DiceValue> destination) => destination.Count() == 3;

  private static bool AllDiceHaveTheSameValue(IEnumerable<DiceValue> destination) =>
    destination.GroupBy(v => v).Count() == 1;
}

public class DiceAreStraight : Validator {
  private readonly IEnumerable<DiceValue> _dice;

  public DiceAreStraight(Dice dice) {
    _dice = dice.DiceValues;
  }

  public override ValidationResult IsSatisfied() =>
    new(_dice.Count() == 4 && AllDiceHaveTheSameValue(_dice),
      "Dice are not a straight");

  private static bool AllDiceHaveTheSameValue(IEnumerable<DiceValue> destination) =>
    destination.GroupBy(v => v).Count() == 1;
}

public class PlayerIsInTurn : Validator {
  private readonly int       _playerId;
  private readonly GameState _state;

  public PlayerIsInTurn(GameState state, int playerId) {
    _state    = state;
    _playerId = playerId;
  }

  public override ValidationResult IsSatisfied() =>
    new(_state.PlayerInTurn == _playerId,
      new V1.PlayedOutOfTurn(_playerId, _state.PlayerInTurn));
}

public class PlayerCanPass : Validator {
  private readonly int  _playerId;
  private readonly Game _game;

  public PlayerCanPass(Game game, int playerId) {
    _game     = game;
    _playerId = playerId;
  }

  public override ValidationResult IsSatisfied() =>
    new(_game.Current.LastEventsWere(typeof(V2.DiceRolled)) ||
        _game.Current.LastEventsWere(typeof(V2.DiceKept)),
      new V1.PassedWithoutRolling(_playerId));
}

public static class EnumerableExtensions {
  public static bool LastEventsWere<T>(
    this IEnumerable<T> events,
    IList<Type>         expectedEvents) {
    var itemList = events
      .Where(i => i is not IErrorEvent)
      .Select(e => e.GetType())
      .Reverse()
      .ToList();

    return !expectedEvents.Where((t, index) => itemList[index] != t).Any();
  }
  
  public static bool LastEventsWere<T>(
    this IEnumerable<T> events,
    Type         expectedEvent) {
    return events.LastEventsWere(new[] { expectedEvent });
  }
}