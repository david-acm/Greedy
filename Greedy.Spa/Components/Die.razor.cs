using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Greedy.Spa.Components;

public partial class Die {
  private (int, int, int) _rotation;
  private string          _id;
  private double          _scale  = 1;
  private DiceValue       _number = DiceValue.None;

  [Parameter] public DiceValue DiceValue { get; set; }

  private double AngleFor(char a) => a switch
  {
    'x' => _rotation.Item1,
    'y' => _rotation.Item2,
    'z' => _rotation.Item3,
    _   => 0
  };

  [Parameter] public int     Size  { get; set; } = 50;
  [Parameter] public string? Class { get; set; }

  [Parameter] public          bool                IsDragging          { get; set; }
  [Inject] public ILogger<Die>        Logger              { get; set; }
  [Inject] public IRotationCalculator _rotationCalculator { get; set; }


  protected override async Task OnInitializedAsync() {
    _id = new string(Guid.NewGuid().ToString().Where(c => !char.IsDigit(c)).ToArray());
    if(IsDragging)
      RotateToValue();
    await base.OnInitializedAsync();
  }

  protected override Task OnParametersSetAsync() {
    if (_number != DiceValue.None && _number != DiceValue)
    {
      RotateToValue();
    }

    return base.OnParametersSetAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender) {
    if (firstRender)
    {
      await DelayedRotateToValue();
      // RotateToValue();
    }

    await base.OnAfterRenderAsync(firstRender);
  }

  private async Task DelayedRotateToValue() {
    _ = new Timer(async _ =>
    {
      await InvokeAsync(async () =>
      {
        RotateToValue();
        StateHasChanged();
      });
    }, null, 0, -1);
  }

  private void RotateToValue() {
    _number = DiceValue;
    var rotation = _rotationCalculator.CalculateFor(_number, !IsDragging);
    SetRotationTo(rotation);
  }

  private void SetRotationTo((int, int, int) rotation) {
    _rotation = rotation;
  }

  private void MouseLeave(MouseEventArgs e) {
    var (x, y, z) = _rotation;
    SetRotationTo((x, y - 10, z - 10));
    Scale(1);
    StateHasChanged();
  }

  private void MouseEnter(MouseEventArgs e) {
    var (x, y, z) = _rotation;
    SetRotationTo((x, y + 10, z + 10));
    Scale(1.4);
    StateHasChanged();
  }

  private void Scale(double scale) {
    _scale = scale;
  }
}