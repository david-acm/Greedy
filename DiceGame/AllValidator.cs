namespace DiceGame;

public class AllValidator : Validator {
  private readonly List<Validator> _validators = new();

  public static AllValidator ValidateThat(Validator validator) {
    var andValidator = new AllValidator();
    andValidator.And(validator);
    return andValidator;
  }

  public override ValidationResult IsSatisfied() {
    var firstFailingValidator = _validators.FirstOrDefault(v => !v.IsSatisfied());
    return firstFailingValidator?.IsSatisfied()
           ?? new ValidationResult(true, string.Empty);
  }
}