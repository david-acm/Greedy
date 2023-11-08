using Microsoft.AspNetCore.Components;

namespace Greedy.Spa.Components; 

public partial class DraggableDice {
  [Parameter]
  public List<DiceValue> DiceValues { get; set; }
}