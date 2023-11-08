using FluentAssertions;
using Greedy.Spa.Components;

namespace Greedy.SpaTests;

public class CalculateForShould {
  [Theory]
  [InlineData(1, 105, 0,   15)]
  [InlineData(3, 15,  255, 0)]
  [InlineData(2, 15,  165, 0)]
  [InlineData(4, 15,  345, 0)]
  [InlineData(5, 15,  75,  0)]
  [InlineData(6, 285, 0,   345)]
  public void ReturnTheCorrectAnglesFor(
    int value,
    int xExpected,
    int yExpected,
    int zExpected) {
    // Arrange
    var sut = new RotationCalculator();

    // Act
    var (x, y, z) = sut.CalculateFor(DiceValue.FromValue(value));

    // Assert
    (
        (x + 720) % 360,
        (y + 720) % (yExpected == 0 ? 90 : 360),
        (z + 720) % (zExpected == 0 ? 90 : 360)).Should()
      .Be((xExpected, yExpected, zExpected));
  }
}