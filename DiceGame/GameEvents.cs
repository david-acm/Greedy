namespace DiceGame;

public record Dice(IEnumerable<DiceValue> DiceValues) {
  public static Dice FromNewThrow(IRandom randomizer, int diceToThrow) {
    var dice = new List<DiceValue>();
    for (var i = 1; i <= diceToThrow; i++)
    {
      dice.Add((DiceValue)randomizer.Next(1, 6));
    }

    return new Dice(dice);
  }

  public static Dice FromValues(IEnumerable<int> values) {
    var diceList = values.ToList();
    if (diceList.Count > 6) throw new ArgumentOutOfRangeException($"Can't throw more than 6 dice. Found: {diceList}");
    return new Dice(diceList.Select(d => (DiceValue)d));
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

public enum DiceValue {
  One = 1, 
  Two,
  Three,
  Four,
  Five,
  Six
}