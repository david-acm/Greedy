using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Greedy.Spa.Components;

public partial class Draggable {
  [CascadingParameter]
  public Position MousePosition
  {
    get => _mousePosition - _offset;
    set
    {
      _mousePosition = value;
      if(_mousedown)
        Move();
    }
  }

  [Parameter]
  public Position InitialPosition
  {
    set
    {
      _initialPosition = value;
    }
  }

  private Position _lastPosition = (0, 0);
  private Position _offset       = (0, 0);
  private bool     _mousedown;
  private string   _id;
  private Position _mousePosition = (0, 0);
  private Position _position      = (0, 0);
  private Position _initialPosition;

  [Inject] public ILogger<Die> Logger { get; set; }

  [Parameter] public RenderFragment? ChildContent { get; set; }

  protected override async Task OnInitializedAsync() {
    _id           = new string(Guid.NewGuid().ToString().Where(c => !char.IsDigit(c)).ToArray());
    _lastPosition = _initialPosition;
    _position     = _initialPosition;
  }


  private void HanldeMouseDown(MouseEventArgs e) {
    if (e.Button != 0) return;
    _mousedown = true;
    _offset    = (e.ClientX, e.ClientY) - _lastPosition;
    Logger.LogInformation(nameof(HanldeMouseDown));
  }

  private void HandleMouseUp(MouseEventArgs e) {
    if (e.Button != 0) return;
    _mousedown    = false;
    _lastPosition = (e.ClientX, e.ClientY) - _offset;
    _offset       = (0, 0);
    Logger.LogInformation(nameof(HandleMouseUp));
  }

  private void Move() {
    if (!_mousedown) return;
    _position = _mousePosition - _offset;
  }
}

public class Position {
  private readonly double _x;
  private readonly double _y;

  public double PositionFor(char a) => a switch
  {
    'x' => _x,
    'y' => _y
  };

  public Position(double x, double y) {
    _x = x;
    _y = y;
  }

  public static Position PositionFrom(double x, double y) =>
    new(x, y);

  public static implicit operator (double, double)(Position p) => (p._x, p._y);

  public static implicit operator Position((double, double) p) => PositionFrom(p.Item1, p.Item2);

  public static Position operator +(Position p1, Position p2) => PositionFrom(p1._x + p2._x, p1._y + p2._y);

  public static Position operator -(Position p1, Position p2) => PositionFrom(p1._x - p2._x, p1._y - p2._y);
}