namespace Greedy.Spa.Components;

public interface IRotationCalculator {
  (int, int, int) CalculateFor(DiceValue diceValue, bool randomSpin = false);
}