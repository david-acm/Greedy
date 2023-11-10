using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Greedy.Spa.Components;

public partial class Die {
  private string          _id     = new string(Guid.NewGuid().ToString().Where(c => !char.IsDigit(c)).ToArray());
  private DiceValue       _number = DiceValue.None;
  private (int, int, int) _rotation;
  private double          _scale = 1;

  [Parameter]
  public DiceValue DiceValue { get; set; }

  [Parameter]
  public int Size { get; set; } = 50;

  [Parameter]
  public string? Class { get; set; }

  [Parameter]
  public bool IsDragging { get; set; }

  [Inject]
  public ILogger<Die> Logger { get; set; }

  [Inject]
  public IRotationCalculator _rotationCalculator { get; set; }

  private double AngleFor(char a) => a switch
  {
    'x' => _rotation.Item1,
    'y' => _rotation.Item2,
    'z' => _rotation.Item3,
    _   => 0
  };

  protected override async Task OnInitializedAsync()
  {
    if (IsDragging)
      RotateToValue();
    await base.OnInitializedAsync();
  }

  protected override Task OnParametersSetAsync()
  {
    if (_number != DiceValue.None && _number != DiceValue) RotateToValue();

    return base.OnParametersSetAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender) await DelayedRotateToValue();
    // RotateToValue();
    await base.OnAfterRenderAsync(firstRender);
  }

  private async Task DelayedRotateToValue() =>
    _ = new Timer(async _ =>
    {
      await InvokeAsync(async () =>
      {
        RotateToValue();
        StateHasChanged();
      });
    }, null, 0, -1);

  private void RotateToValue()
  {
    _number = DiceValue;
    var rotation = _rotationCalculator.CalculateFor(_number, !IsDragging);
    SetRotationTo(rotation);
  }

  private void SetRotationTo((int, int, int) rotation) =>
    _rotation = rotation;

  private void MouseLeave(MouseEventArgs e)
  {
    (int x, int y, int z) = _rotation;
    SetRotationTo((x, y - 10, z - 10));
    Scale(1);
    StateHasChanged();
  }

  private void MouseEnter(MouseEventArgs e)
  {
    (int x, int y, int z) = _rotation;
    SetRotationTo((x, y + 10, z + 10));
    Scale(1.4);
    StateHasChanged();
  }

  private void Scale(double scale) =>
    _scale = scale;
}