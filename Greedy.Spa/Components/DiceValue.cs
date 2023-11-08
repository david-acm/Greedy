using Ardalis.SmartEnum;

namespace Greedy.Spa.Components;

public sealed class DiceValue : SmartEnum<DiceValue, int> {
  public static readonly DiceValue None = new("n", 0);
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