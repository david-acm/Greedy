namespace DiceGame;

public record Dice(DiceValue DiceOne, DiceValue DiceTwo, DiceValue DiceThree, DiceValue DiceFour, DiceValue DiceFive,
  DiceValue DiceSix) {
  public static Dice FromNewThrow() {
    var random = new Random();
    var dice = new List<DiceValue>();
    for (var i = 0; i <= 6; i++)
    {
      dice.Add((DiceValue)random.Next(1, 6));
    }

    return new Dice(
      dice[0],
      dice[1],
      dice[2],
      dice[3],
      dice[4],
      dice[5]
    );
  }
}

public static class GameEvents {
  public record PlayedOutOfTurn(int TriedToPlay, int ExpectedPlayer);

  public record GameStarted(int Id);

  public record PlayerJoined(int Id, string Name);

  public record DiceThrown(int PlayerId, int Die1, int Die2, int Die3, int Die4, int Die5, int Die6);
}

public enum DiceValue {
  One = 1, 
  Two,
  Three,
  Four,
  Five,
  Six
}