using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Greedy.Spa.Components;

public partial class DragabbleDice {
  private void ItemUpdated(MudItemDropInfo<DropItem> dropItem) =>
    dropItem.Item.Identifier = dropItem.DropzoneIdentifier;

  [Parameter]
  public List<DiceValue> DiceValues { get; set; }

  private List<DropItem> _items = new();

  protected override void OnInitialized()
    => _items = DiceValues.Select(d => new DropItem
    {
      Value      = d,
      Identifier = "Rolled"
    }).ToList();

  protected override void OnParametersSet()
  {
    int itemsCount = Math.Min(_items.Count, DiceValues.Count);
    for (int i = 0; i < itemsCount; i++)
    {
      _items.ElementAt(i).Value = DiceValues.ElementAt(i);
    }

    base.OnParametersSet();
  }

  public class DropItem {
    public DiceValue Value      { get; set; }
    public string?   Identifier { get; set; }
  }
}