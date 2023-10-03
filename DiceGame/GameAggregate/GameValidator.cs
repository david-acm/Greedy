using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.GameAggregate;

public static class GameValidator {
  public static void EnsurePreconditions(GameState state, object @event) {
    var valid = @event switch
    {
      GameStarted e => Validate(
        state.GameStage == GameStage.None, new GameAlreadyStarted(e.Id)),
      PlayerJoined => Validate(
        state.GameStage == GameStage.Started, new GameHasNotStarted(state.GameStage)),
      DiceThrown e => Validate(
        PlayerInTurn(state, e.PlayerId), new PlayedOutOfTurn(e.PlayerId, state.PlayerInTurn)),
      TurnPassed e => Validate(
        PlayerInTurn(state, e.PlayerId),
        new PlayedOutOfTurn(e.PlayerId, state.PlayerInTurn)),
      DiceKept e =>
        new PlayerIsInTurn(state, e.PlayerId)
          .And(
            new PlayerHasThoseDice(GetDice(e), state))
          .And
          (
            new CanKeepDice(GetDice(e))
            // new DiceAreOnesOrFives(GetDice(e))
            //   .Or(
            //     new DiceAreTrips(GetDice(e)))
            //   .Or(
            //     new DiceAreStair(GetDice(e)))
          )
          .IsSatisfied(),

      _ => Validate(false, $"No validation performed for event {@event}")
    };

    if (valid) return;

    throw new PreconditionsFailedException(valid.FailedValidationEvent.ToString()!, valid.FailedValidationEvent);
  }

  private static Dice GetDice(DiceKept e) =>
    Dice.FromValues(e.Dice.ToList());

  private static bool PlayerInTurn(GameState state, int playerId) =>
    state.PlayerInTurn == playerId;

  private static ValidationResult Validate(bool validation, object failedValidationEvent) =>
    new(validation, failedValidationEvent);
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

internal record GameAlreadyStarted(int Id);

internal record GameHasNotStarted(GameStage GameStage);

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

public class PlayerIsInTurn : Validator {
  private readonly int       _playerId;
  private readonly GameState _state;

  public PlayerIsInTurn(GameState state, int playerId) {
    _state    = state;
    _playerId = playerId;
  }

  public override ValidationResult IsSatisfied() =>
    new(_state.PlayerInTurn == _playerId,
      new PlayedOutOfTurn(_playerId, _state.PlayerInTurn));
}