using FluentAssertions;
using Greedy.GameAggregate;

namespace Greedy.Tests.Framework;

public class LoadShould {
  [Fact]
  public void RestoreGameStateFromEvents()
  {
    // Arrange
    var game   = new Game();
    var events = new[] { new GameEvents.V1.GameStarted(1) };

    // Act
    game.Load(events);

    // Assert
    game.State.GameStage.Should().Be(GameStage.Rolling);
    game.State.Id.Should().Be((GameId)events[0].Id);
  }
}