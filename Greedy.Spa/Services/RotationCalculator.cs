namespace Greedy.Spa.Components;

public class RotationCalculator : IRotationCalculator {
  public (int, int, int) CalculateFor(DiceValue diceValue, bool randomSpin = true) {
    return diceValue.Value switch
    {
      1 => AddSpinsTo((105, 0, 15), randomSpin),
      2 => AddSpinsTo((15, 165, 0), randomSpin),
      3 => AddSpinsTo((15, 255, 0), randomSpin),
      4 => AddSpinsTo((15, 345, 0), randomSpin),
      5 => AddSpinsTo((15, 75, 0), randomSpin),
      6 => AddSpinsTo((285, 0, 345), randomSpin),
      _ => ((0, 0, 0))
    };
  }

  private static (int, int, int) AddSpinsTo((int x, int y, int z) rotation, bool randomSpin) {
    var rnd         = new Random();
    var spinDegrees = (randomSpin ? rnd.Next(-2, 2) : 0) * 360;
    var (x, y, z) = rotation;
    return (x + spinDegrees, y + spinDegrees, z + spinDegrees);
  }
}