namespace Greedy.Spa.Components;

public interface IRotationCalculator {
  (int, int, int) CalculateFor(DiceValue diceValue);
}

public class RotationCalculator : IRotationCalculator {
  public (int, int, int) CalculateFor(DiceValue diceValue) {
    return diceValue switch
    {
      DiceValue.One   => AddSpinsTo(105, 0,   15),
      DiceValue.Two   => AddSpinsTo(15,  165, 0),
      DiceValue.Three => AddSpinsTo(15,  255, 0),
      DiceValue.Four  => AddSpinsTo(15,  345, 0),
      DiceValue.Five  => AddSpinsTo(15,  75,  0),
      DiceValue.Six   => AddSpinsTo(285, 0,   345),
      _               => (0, 0, 0)
    };
  }

  private static (int, int, int) AddSpinsTo(int x, int y, int z) {
    var rnd         = new Random();
    var spinDegrees = rnd.Next(-2, 2) * 360;
    return (x + spinDegrees, y + spinDegrees, z + spinDegrees);
  }
}