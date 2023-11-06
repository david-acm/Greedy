using FluentAssertions;
using Greedy.Spa.Components;

namespace Greedy.SpaTests;

public class CalculateForShould {
  [Theory]
  [InlineData(DiceValue.One,   105, 0,   15)]
  [InlineData(DiceValue.Three, 15,  255, 0)]
  [InlineData(DiceValue.Two,   15,  165, 0)]
  [InlineData(DiceValue.Four,  15,  345, 0)]
  [InlineData(DiceValue.Five,  15,  75,  0)]
  [InlineData(DiceValue.Six,   285, 0,   345)]
  public void ReturnTheCorrectAnglesFor(
    DiceValue value,
    int       xExpected,
    int       yExpected,
    int       zExpected) {
    // Arrange
    var sut = new RotationCalculator();

    // Act
    var (x, y, z) = sut.CalculateFor(value);

    // Assert
    (
        (x + 720) % 360,
        (y + 720) % (yExpected == 0 ? 90 : 360),
        (z + 720) % (zExpected == 0 ? 90 : 360)).Should()
      .Be((xExpected, yExpected, zExpected));
  }
}