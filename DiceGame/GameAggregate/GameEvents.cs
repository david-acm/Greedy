using Ardalis.SmartEnum;

namespace DiceGame.GameAggregate;

public record Dice(IEnumerable<DiceValue> DiceValues) {
  public static Dice FromNewRoll(IRandom randomizer, int diceToRoll) {
    var dice = new List<DiceValue>();
    for (var i = 1; i <= diceToRoll; i++)
      dice.Add(DiceValue.FromValue(randomizer.Next(1,
        6)));

    return new Dice(dice);
  }

  public static Dice FromValues(IEnumerable<int> values) {
    var valueList = values.ToList();
    if (valueList.Count > 6) throw new ArgumentOutOfRangeException($"Can't Roll more than 6 dice. Found: {valueList}");
    return new Dice(valueList.ToDiceValues());
  }
}

public interface IRandom {
  int Next(int minValue, int maxValue);
}

public static class GameEvents {
  public record PlayedOutOfTurn(int TriedToPlay, int ExpectedPlayer);

  public record GameStarted(int Id);

  public record PlayerJoined(int Id, string Name);

  public record DiceRolled(int PlayerId, int[] Dice);
}

public sealed class DiceValue : SmartEnum<DiceValue, int> {
  public static readonly DiceValue One = new("⚀", 1);

  public static readonly DiceValue Two = new("⚁", 2);

  public static readonly DiceValue Three = new("⚂", 3);

  public static readonly DiceValue Four = new("⚃", 4);

  public static readonly DiceValue Five = new("⚄", 5);

  public static readonly DiceValue Six = new("⚅", 6);

  private DiceValue(string name, int value) : base(name,
    value) {
  }
}