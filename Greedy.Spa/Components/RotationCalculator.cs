namespace Greedy.Spa.Components;

public interface IRotationCalculator {
  (int,int,int) CalculateFor(DiceValue diceValue);
}

public class RotationCalculator : IRotationCalculator {
  public (int,int,int) CalculateFor(DiceValue diceValue) {
    var rnd         = new Random();
    var spinDegrees = rnd.Next(0, 0) * 360;
    return diceValue switch
    {
      DiceValue.One   => (105 + spinDegrees, 0, 15  + spinDegrees),
      DiceValue.Three => (15  + spinDegrees, 255    + spinDegrees, 0),
      DiceValue.Two   => (15  + spinDegrees, 165    + spinDegrees, 0),
      DiceValue.Four  => (15  + spinDegrees, 345    + spinDegrees, 0),
      DiceValue.Five  => (15  + spinDegrees, 75     + spinDegrees, 0),
      DiceValue.Six   => (-75 + spinDegrees, 0, -15 + spinDegrees),
      _               => (0,0,0)
    };
  }
}