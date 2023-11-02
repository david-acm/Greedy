using Microsoft.AspNetCore.Components;

namespace Greedy.Spa.Components; 

public partial class Dice {
  [Parameter]
  public List<DiceValue> DiceValues { get; set; }
}

public enum DiceValue {
  None,
  One,
  Two,
  Three,
  Four,
  Five,
  Six,
}