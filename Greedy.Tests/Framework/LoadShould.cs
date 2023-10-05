using Greedy.GameAggregate;
using FluentAssertions;
using static Greedy.GameAggregate.Command;

namespace Greedy.Tests.Framework;

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
    game.State.Id.Should().Be((GameId)events[0].Id);
  }
}