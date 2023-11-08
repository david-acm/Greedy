namespace Greedy.Spa.Components;

public record Position {
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

  public double DistanceFrom(Position p) => Math.Sqrt(Math.Pow((this - p)._x, 2) + Math.Pow((this - p)._y, 2));
  public static implicit operator (double, double)(Position p) => (p._x, p._y);

  public static implicit operator Position((double, double) p) => PositionFrom(p.Item1, p.Item2);

  public static Position operator +(Position p1, Position p2) => PositionFrom(p1._x + p2._x, p1._y + p2._y);

  public static Position operator -(Position p1, Position p2) => PositionFrom(p1._x - p2._x, p1._y - p2._y);
  public static Position operator /(Position p1, Position p2) => PositionFrom(p1._x / p2._x, p1._y / p2._y);
  public static Position operator *(Position p1, Position p2) => PositionFrom(p1._x * p2._x, p1._y * p2._y);
  public static bool operator >(Position     p1, Position p2) => p1._x > p2._x && p1._y > p2._y;
  public static bool operator <(Position     p1, Position p2) => p1._x < p2._x && p1._y < p2._y;
}