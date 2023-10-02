namespace DiceGame;

public abstract class Validator {
  public abstract ValidationResult IsSatisfied();

  public OrValidator Or(Validator validator) =>
    new(this, validator);

  public AndValidator And(Validator validator) =>
    new(this, validator);
}

public class AndValidator : Validator {
  private readonly Validator _left;
  private readonly Validator _right;

  public AndValidator(Validator left, Validator right) {
    _left  = left;
    _right = right;
  }

  public override ValidationResult IsSatisfied() {
    if (!_left.IsSatisfied())
      return _left.IsSatisfied() with { IsValid = _left.IsSatisfied() };
    if (_right.IsSatisfied())
      return new ValidationResult(true, string.Empty);

    return _right.IsSatisfied() with { IsValid = _right.IsSatisfied() };
  }
}

public class OrValidator : Validator {
  private readonly Validator _left;
  private readonly Validator _right;

  public OrValidator(Validator left, Validator right) {
    _left  = left;
    _right = right;
  }

  public override ValidationResult IsSatisfied() {
    if (_left.IsSatisfied() || _right.IsSatisfied())
      return new ValidationResult(true, string.Empty);

    if (!_left.IsSatisfied())
      return _left.IsSatisfied() with { IsValid = _right.IsSatisfied() };

    return _right.IsSatisfied() with { IsValid = _right.IsSatisfied() };
  }
}