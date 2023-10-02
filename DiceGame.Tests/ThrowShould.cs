using FluentAssertions;
using Xunit.Abstractions;
using static DiceGame.DiceValue;
using static DiceGame.GameEvents;

namespace DiceGame.Tests;

public class ThrowShould : GameWithThreePlayersTest {
  public ThrowShould(ITestOutputHelper output) : base(output) {
    
    SetupDiceToThrow(new List<int>() { 1, 2, 3, 4, 5, 6 });
  }

  [Fact]
  public void AllowPlayerToThrow() {
    // Act
    Game.ThrowDice(1);
    Game.Pass(1);

    // Assert
    State.Throws.Should().HaveCount(1);
    var diceThrown = Events.Where(e => e is DiceThrown)
      .Should().HaveCount(1).And.Subject;
    diceThrown.Should()
      .ContainSingle(e =>
        ((DiceThrown)e).PlayerId == 1);
  }

  [Fact]
  public void NotAllowPlayerToThrowOutOfTurn() {
    // Act
    var action = () => Game.ThrowDice(2);

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Throws.Should().HaveCount(0);
    var playedOutOfTurn = Events
      .Where(e => e is PlayedOutOfTurn).Should().ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((PlayedOutOfTurn)e).TriedToPlay == 2 &&
        ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void NotAllowNextPlayerToPlayUntilPlayerPasses() {
    // Act
    Game.ThrowDice(1);

    var action = () => Game.ThrowDice(2);

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Throws.Should().HaveCount(1);
    var playedOutOfTurn = Events
      .Where(e => e is PlayedOutOfTurn).Should().ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((PlayedOutOfTurn)e).TriedToPlay == 2 &&
        ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void ThrowOnlyAvailableDiceAtTheTableCenter() {
    // Arrange
    SetupDiceToThrow(new List<int>() { 4, 4, 5, 2, 1, 2, 3 });

    // Act
    Game.ThrowDice(1);
    Game.Keep(1, new[] { One });
    SetupDiceToThrow(new List<int>() { 4, 4, 5, 2, 1, 2 });
    Game.ThrowDice(1);
    Game.Keep(1, new[] { Five });

    // Assert
    State.Throws.Should().HaveCount(2);
    State.Throws.Last().Dice.DiceValues.Should().HaveCount(5);
  }
}