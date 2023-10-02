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
    game.Start(new GameId(gameId));

    // Assert
    game.State.GameStage.Should().Be(Started);
    game.State.Id.Should().Be(gameId);
  }

  [Fact]
  public void RaiseGameStartedEvent() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new GameId(1));

    // Assert
    game.Events.Should().Contain(e => e is GameStarted);
  }
  
  [Fact]
  public void NotAllowAGameToStartTwice() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new GameId(1));
    var secondStart = () => game.Start(new GameId(1));

    // Assert
    secondStart.Should().Throw<PreconditionsFailedException>();
  }
}