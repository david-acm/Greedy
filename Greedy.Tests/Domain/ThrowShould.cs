using Greedy.GameAggregate;
using FluentAssertions;
using Xunit.Abstractions;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.DiceValue;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.Tests.Domain;

public class RollShould : GameWithThreePlayersTest {
  public RollShould(ITestOutputHelper output) : base(output) {
  }

  [Fact]
  public void AllowPlayerToRoll() {
    // Act
    Game.RollDice(new RollDice(1, 1));
    Game.PassTurn(new PassTurn(1, 1));

    // Assert
    State.Rolls.Should().HaveCount(1);
    var diceRolled = Events.Where(e => e is DiceRolled)
      .Should()
      .HaveCount(1)
      .And.Subject;
    diceRolled.Should()
      .ContainSingle(e =>
        ((DiceRolled)e).PlayerId == 1);
  }

  [Fact]
  public void NotAllowPlayerToRollOutOfTurn() {
    // Act
    var action = () => Game.RollDice(new RollDice(1, 2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Rolls.Should().HaveCount(0);
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
    Game.RollDice(new RollDice(1, 1));
    SetupDiceToRoll(new List<int>
      { 4, 4, 4, 2, 1, 2, 3 });

    var action = () => Game.RollDice(new RollDice(1, 2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Rolls.Should().HaveCount(1);
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
  public void RollOnlyAvailableDiceAtTheTableCenter() {
    // Arrange
    SetupDiceToRoll(new List<int> { 4, 4, 5, 2, 1, 2, 3 });
    // Act
    Game.RollDice(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { One }));


    SetupDiceToRoll(new List<int> { 4, 4, 5, 2, 1, 2 });

    Game.RollDice(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { Five }));

    // Assert
    State.Rolls.Should().HaveCount(2);
    State.LastRoll!.Dice.DiceValues.Should().HaveCount(5);
  }
}