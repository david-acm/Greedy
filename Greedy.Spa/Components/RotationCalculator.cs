namespace Greedy.Spa.Components;

public interface IRotationCalculator {
  (int, int, int) CalculateFor(DiceValue diceValue);
}

public class RotationCalculator : IRotationCalculator {
  public (int, int, int) CalculateFor(DiceValue diceValue) {
    return diceValue.Value switch
    {
      1   => AddSpinsTo(105, 0,   15),
      2   => AddSpinsTo(15,  165, 0),
      3 => AddSpinsTo(15,  255, 0),
      4  => AddSpinsTo(15,  345, 0),
      5  => AddSpinsTo(15,  75,  0),
      6   => AddSpinsTo(285, 0,   345),
      _               => (0, 0, 0)
    };
  }

  private static (int, int, int) AddSpinsTo(int x, int y, int z) {
    var rnd         = new Random();
    var spinDegrees = rnd.Next(-2, 2) * 360;
    return (x + spinDegrees, y + spinDegrees, z + spinDegrees);
  }
}