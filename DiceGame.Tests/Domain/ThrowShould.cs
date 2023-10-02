using DiceGame.GameAggregate;
using FluentAssertions;
using Xunit.Abstractions;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.DiceValue;
using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.Tests.Domain;

public class ThrowShould : GameWithThreePlayersTest {
  public ThrowShould(ITestOutputHelper output) : base(output) {
  }

  [Fact]
  public void AllowPlayerToThrow() {
    // Act
    Game.ThrowDice(new PlayerId(1));
    Game.Pass(new PlayerId(1));

    // Assert
    State.Throws.Should().HaveCount(1);
    var diceThrown = Events.Where(e => e is DiceThrown)
      .Should()
      .HaveCount(1)
      .And.Subject;
    diceThrown.Should()
      .ContainSingle(e =>
        ((DiceThrown)e).PlayerId == 1);
  }

  [Fact]
  public void NotAllowPlayerToThrowOutOfTurn() {
    // Act
    var action = () => Game.ThrowDice(new PlayerId(2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Throws.Should().HaveCount(0);
    var playedOutOfTurn = Events
      .Where(e => e is PlayedOutOfTurn)
      .Should()
      .ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((PlayedOutOfTurn)e).TriedToPlay    == 2 &&
        ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void NotAllowNextPlayerToPlayUntilPlayerPasses() {
    // Act
    Game.ThrowDice(new PlayerId(1));
    SetupDiceToThrow(new List<int>
      { 4, 4, 4, 2, 1, 2, 3 });

    var action = () => Game.ThrowDice(new PlayerId(2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Throws.Should().HaveCount(1);
    var playedOutOfTurn = Events
      .Where(e => e is PlayedOutOfTurn)
      .Should()
      .ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((PlayedOutOfTurn)e).TriedToPlay    == 2 &&
        ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void ThrowOnlyAvailableDiceAtTheTableCenter() {
    // Arrange
    SetupDiceToThrow(new List<int>
      { 4, 4, 5, 2, 1, 2, 3 });

    // Act
    Game.ThrowDice(new PlayerId(1));
    Game.Keep(new Keep(1, new[] { One }));
    SetupDiceToThrow(new List<int>
      { 4, 4, 5, 2, 1, 2 });
    Game.ThrowDice(new PlayerId(1));
    Game.Keep(new Keep(1, new[] { Five }));

    // Assert
    State.Throws.Should().HaveCount(2);
    State.LastThrow!.Dice.DiceValues.Should().HaveCount(5);
  }
}