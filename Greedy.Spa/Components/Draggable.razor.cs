using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Greedy.Spa.Components;

public partial class Draggable {
  [CascadingParameter]
  public Position MousePosition
  {
    get => _mousePosition - _dragOffset;
    set
    {
      _mousePosition = value;
      if (_mousedown)
        Move();
    }
  }

  [Parameter]
  public Position InitialPosition
  {
    set { _initialPosition = value; }
  }

  private bool   _mousedown;
  private string _id;

  private Position _dragOffset      = (0, 0);
  private Position _lastPosition    = (0, 0);
  private Position _mousePosition   = (0, 0);
  private Position _position        = (0, 0);
  private Position _initialPosition = (0, 0);

  [Inject] public ILogger<Die> Logger { get; set; }

  [Parameter] public RenderFragment? ChildContent { get; set; }

  protected override async Task OnInitializedAsync() {
    _id = new string(Guid.NewGuid().ToString().Where(c => !char.IsDigit(c)).ToArray());

    _lastPosition = _initialPosition;
    _position     = _initialPosition;
  }

  private void HandleMouseDown(MouseEventArgs e) {
    if (e.Button != 0) return;
    _mousedown  = true;
    _dragOffset = (e.ClientX, e.ClientY) - _lastPosition;
    Logger.LogInformation(nameof(HandleMouseDown));
  }

  private void HandleMouseUp(MouseEventArgs e) {
    if (e.Button != 0) return;
    _mousedown    = false;
    _lastPosition = (e.ClientX, e.ClientY) - _dragOffset;
    _dragOffset   = (0, 0);
    Logger.LogInformation(nameof(HandleMouseUp));
  }

  private void Move() {
    if (!_mousedown) return;
    var newPosition = _mousePosition - _dragOffset;
    if (HasReachedBorder(newPosition))
    {
      _position = EaseMovement(newPosition);
      return;
    }

    _position = newPosition;
  }

  private Position EaseMovement(Position position) {
    var easeDistance       = 50;
    var newX       = position.PositionFor('x');
    var newY       = position.PositionFor('y');
    
    var normalized = position / (easeDistance, easeDistance);
    _position = position / ((1, 1) - normalized);
    
    var x = newX > 0 ? newX : newX / (1 - newX / easeDistance);
    var y = newY > 0 ? newY : newY / (1 - newY / easeDistance);
    
    return (x, y);
  }

  private static bool HasReachedBorder(Position position) {
    var newX = position.PositionFor('x');
    var newY = position.PositionFor('y');
    return !(newX > 0) || !(newY > 0);
  }
}