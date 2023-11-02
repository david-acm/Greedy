using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Greedy.Spa.Components;

public partial class Die {
  private int       _z      = 0;
  private int       _y      = 0;
  private int       _x      = 0;
  private string    _id;
  private double    _scale;
  private DiceValue _number = DiceValue.None;

  [Parameter] 
  public DiceValue DiceValue { get; set; }

  [Parameter] 
  public int          Size   { get; set; }
  [Inject]   
  public ILogger<Die> Logger { get; set; }
  [Inject]
  public IRotationCalculator _rotationCalculator { get; set; }
  

  protected override async Task OnInitializedAsync() {
    _id = new string(Guid.NewGuid().ToString().Where(c => !char.IsDigit(c)).ToArray());
    await base.OnInitializedAsync();
  }

  protected override Task OnParametersSetAsync() {
    if (_number is not DiceValue.None && _number != DiceValue)
      RotateToValue();
    return base.OnParametersSetAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender) {
    if (firstRender)
    {
      await DelayedRotateToValue();
      return;
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
    _number                = DiceValue;
    var (x, y, z) = _rotationCalculator.CalculateFor(_number);
    RotateDegrees(x, y, z);
  }

  private void RotateDegrees(int x, int y, int z) {
    _x = x;
    _z = z;
    _y = y;
  }

  private void MouseEnter(MouseEventArgs e) {
    RotateDegrees(_x, _y + 10, _z + 10);
    Scale(1.2);
    StateHasChanged();
    Logger.LogInformation($"Enter");
  }

  private void Scale(double scale) {
    _scale = scale;
  }

  private void MouseLeave(MouseEventArgs e) {
    RotateDegrees(_x, _y - 10, _z - 10);
    Scale(1);
    StateHasChanged();
    Logger.LogInformation($"Leave");
  }
}