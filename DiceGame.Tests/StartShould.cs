using FluentAssertions;
using static DiceGame.GameEvents;
using static DiceGame.GameStage;

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
    game.State.GameStage.Should().Be(Started);
    game.State.Id.Should().Be(gameId);
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
  
  
  [Fact]
  public void NotAllowAGameToStartTwice() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(gameId: 1);
    var secondStart = () => game.Start(gameId: 1);

    // Assert
    secondStart.Should().Throw<PreconditionsFailedException>();
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
    game.State.GameStage.Should().Be(Started);
    game.State.Id.Should().Be(events[0].Id);
  }
}