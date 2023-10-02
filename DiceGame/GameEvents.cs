using Ardalis.SmartEnum;

namespace DiceGame;

public record Dice(IEnumerable<DiceValue> DiceValues) {
  public static Dice FromNewThrow(IRandom randomizer, int diceToThrow) {
    var dice = new List<DiceValue>();
    for (var i = 1; i <= diceToThrow; i++)
    {
      dice.Add(DiceValue.FromValue(randomizer.Next(1,
        6)));
    }

    return new Dice(dice);
  }

  public static Dice FromValues(IEnumerable<int> values) {
    var valueList = values.ToList();
    if (valueList.Count > 6) throw new ArgumentOutOfRangeException($"Can't throw more than 6 dice. Found: {valueList}");
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

  public record DiceThrown(int PlayerId, int[] Dice);
}

public sealed class DiceValue : SmartEnum<DiceValue, int> {
  public static readonly DiceValue One   = new DiceValue("⚀", 1);

  public static readonly DiceValue Two   = new DiceValue("⚁", 2);

  public static readonly DiceValue Three = new DiceValue("⚂", 3);

  public static readonly DiceValue Four  = new DiceValue("⚃", 4);

  public static readonly DiceValue Five  = new DiceValue("⚄", 5);

  public static readonly DiceValue Six   = new DiceValue("⚅", 6);

  private DiceValue(string name, int value) : base(name,
    value) {
  }
}