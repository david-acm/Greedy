using FluentAssertions;

namespace DiceGame.Tests;

public class PassShould {
  
  [Fact]
  public void AllowPlayerToPass() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(1);
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");
    game.JoinPlayer(3, "German");
    game.ThrowDice(1);
    game.Pass(1);
      
    // Assert
    game.Events.Where(e => e is TurnPassed)
      .Should().HaveCount(1);
  }
  
  [Fact]
  public void NotAllowPlayerNotInTurnToPass() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(1);
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");
    game.JoinPlayer(3, "German");
    game.ThrowDice(1);
    var action = () => game.Pass(2);

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    game.State.Throws.Should().HaveCount(1);
    var playedOutOfTurn = game.Events
      .Where(e => e is GameEvents.PlayedOutOfTurn).Should().ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((GameEvents.PlayedOutOfTurn)e).TriedToPlay == 2 &&
        ((GameEvents.PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }
}