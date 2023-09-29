using FluentAssertions;

namespace DiceGame.Tests;

public class ThrowShould {
  [Fact]
  public void AddThrowToGame() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(1);
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");
    game.ThrowDice(1);

    // Assert
    game.State.Throws.Should().HaveCount(1);
    game.Events.Should().ContainSingle(e => e is GameEvents.DiceThrown && ((GameEvents.DiceThrown)e).PlayerId == 1);
  }
}