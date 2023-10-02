using DiceGame.GameAggregate;
using FluentAssertions;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.GameEvents;
using static DiceGame.GameAggregate.GameStage;

namespace DiceGame.Tests.Domain;

public class StartShould {
  [Fact]
  public void ChangeStateToStarted() {
    // Arrange
    var game = new Game();

    // Act
    var gameId = 1;
    game.Start(new StartGame(gameId));

    // Assert
    game.State.GameStage.Should().Be(Started);
    game.State.Id.Should().Be(gameId);
  }

  [Fact]
  public void RaiseGameStartedEvent() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new StartGame(1));

    // Assert
    game.Events.Should().Contain(e => e is GameStarted);
  }

  [Fact]
  public void NotAllowAGameToStartTwice() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new StartGame(1));
    var secondStart = () => game.Start(new StartGame(1));

    // Assert
    secondStart.Should().Throw<PreconditionsFailedException>();
  }
}