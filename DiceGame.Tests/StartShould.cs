using FluentAssertions;
using static DiceGame.GameState;

namespace DiceGame.Tests;

public class StartShould {
  [Fact]
  public void ChangeStateToStarted() {
    // Arrange
    var game = new Game();

    // Act
    var gameId = 1; 
    game.Start(gameId: gameId);

    // Assert
    game.State.Should().Be(Started);
    game.Id.Should().Be(gameId);
  }

  [Fact]
  public void RaiseGameStartedEvent() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(gameId: 1);

    // Assert
    game.Events.Should().Contain(e => e is GameStarted);
  }
}

public class LoadShould {
  [Fact]
  public void RestoreGameStateFromEvents() {
    // Arrange
    var game = new Game();
    var events = new[] { new GameStarted(1) };

    // Act
    game.Load(events);

    // Assert
    game.State.Should().Be(Started);
    game.Id.Should().Be(events[0].Id);
  }
}