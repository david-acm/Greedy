namespace DiceGame;

public record ValidationResult(bool IsValid, object FailedValidationEvent) {
  public static implicit operator bool(ValidationResult result) => result.IsValid;
}