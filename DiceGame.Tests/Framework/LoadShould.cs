using DiceGame.GameAggregate;
using FluentAssertions;

namespace DiceGame.Tests.Framework;

public class LoadShould {
  [Fact]
  public void RestoreGameStateFromEvents() {
    // Arrange
    var game   = new Game();
    var events = new[] { new GameEvents.GameStarted(1) };

    // Act
    game.Load(events);

    // Assert
    game.State.GameStage.Should().Be(GameStage.Started);
    game.State.Id.Should().Be(events[0].Id);
  }
}