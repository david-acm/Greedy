using FluentAssertions;
using static DiceGame.GameEvents;

namespace DiceGame.Tests;

public class ThrowShould {
  [Fact]
  public void AllowPlayerToThrow() {
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
    game.State.Throws.Should().HaveCount(1);
    var diceThrown = game.Events.Where(e => e is DiceThrown)
      .Should().HaveCount(1).And.Subject;
    diceThrown.Should()
      .ContainSingle(e =>
        ((DiceThrown)e).PlayerId == 1);
  }

    [Fact]
    public void NotAllowPlayerToThrowOutOfTurn() {
      // Arrange
      var game = new Game();

      // Act
      game.Start(1);
      game.JoinPlayer(1, "David");
      game.JoinPlayer(2, "Cristian");
      game.JoinPlayer(3, "German");
      var action = () => game.ThrowDice(2);

      // Assert
      action.Should().Throw<PreconditionsFailedException>();
      game.State.Throws.Should().HaveCount(0);
      var playedOutOfTurn = game.Events
        .Where(e => e is PlayedOutOfTurn).Should().ContainSingle()
        .And.Subject;
      playedOutOfTurn.Should()
        .Satisfy(e =>
          ((PlayedOutOfTurn)e).TriedToPlay == 2 &&
          ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
    }

    [Fact]
    public void NotAllowNextPlayerToPlayUntilPlayerPasses() {
      // Arrange
      var game = new Game();

      // Act
      game.Start(1);
      game.JoinPlayer(1, "David");
      game.JoinPlayer(2, "Cristian");
      game.JoinPlayer(3, "German");
      game.ThrowDice(1);

      var action = () => game.ThrowDice(2);

      // Assert
      action.Should().Throw<PreconditionsFailedException>();
      game.State.Throws.Should().HaveCount(1);
      var playedOutOfTurn = game.Events
        .Where(e => e is PlayedOutOfTurn).Should().ContainSingle()
        .And.Subject;
      playedOutOfTurn.Should()
        .Satisfy(e =>
          ((PlayedOutOfTurn)e).TriedToPlay == 2 &&
          ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
    }
  }