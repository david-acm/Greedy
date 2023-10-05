namespace Greedy.GameAggregate;

public class DefaultRandomProvider : IRandom {
  public int Next(int minValue, int maxValue) =>
    new Random().Next(minValue, maxValue);
}