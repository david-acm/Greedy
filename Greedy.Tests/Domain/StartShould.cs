using Greedy.GameAggregate;
using FluentAssertions;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.GameEvents.V1;
using static Greedy.GameAggregate.GameStage;

namespace Greedy.Tests.Domain;

public class StartShould {
  [Fact]
  public void ChangeStateToStarted() {
    // Arrange
    var game = new Game();

    // Act
    var gameId = 1;
    game.Start(new StartGame(gameId));

    // Assert
    game.State.GameStage.Should().Be(Rolling);
    game.State.Id.Should().Be((GameId)gameId);
  }

  [Fact]
  public void RaiseGameStartedEvent() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new StartGame(1));

    // Assert
    game.Changes.Should().Contain(e => e is GameStarted);
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