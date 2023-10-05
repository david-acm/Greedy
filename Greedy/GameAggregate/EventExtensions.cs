namespace Greedy.GameAggregate;

public static class EventExtensions {
  public static int[] ToPrimitiveArray(this IEnumerable<DiceValue> values) =>
    values.Select(v => v.Value).ToArray();

  public static IEnumerable<DiceValue> ToDiceValues(this IEnumerable<int> values) =>
    values.Select(DiceValue.FromValue);
}