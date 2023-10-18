using FluentAssertions;
using Greedy.Tests.Framework;
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
    State.TableCenter.Should().HaveCount(6);
    var diceRolled = Events.Where(e => e is V2.DiceRolled)
      .Should()
      .HaveCount(1)
      .And.Subject;
    diceRolled.Should()
      .ContainSingle(e =>
        ((V2.DiceRolled)e).PlayerId == 1);
  }

  [Fact]
  public void NotAllowPlayerToRollOutOfTurn() {
    // Act
    var action = () => Game.RollDice(new RollDice(1, 2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    var playedOutOfTurn = Events.Should().ContainSingleEvent<V1.PlayedOutOfTurn>();
    playedOutOfTurn.Should()
      .Be(new V1.PlayedOutOfTurn(2 , 1));
  }

  
  [Fact]
  public void NotAllowPlayerToRollTwiceBeforeKeepingSomeDice() {
    // Act
    Game.RollDice(new RollDice(1, 1));
    SetupDiceToRoll(new List<int>
      { 4, 4, 4, 2, 1, 2, 3 });
    var action = () => Game.RollDice(new RollDice(1, 1));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.TableCenter.Should().HaveCount(6);
    var playedOutOfTurn = Events.Should().ContainSingleEvent<V1.RolledTwice>();
    playedOutOfTurn!.Player.Should().Be(1);
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
    var playedOutOfTurn = Events
      .Where(e => e is V1.PlayedOutOfTurn)
      .Should()
      .ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((V1.PlayedOutOfTurn)e).TriedToPlay    == 2 &&
        ((V1.PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void RollOnlyAvailableDiceAtTheTableCenter() {
    // Arrange
    SetupDiceToRoll(new List<int> { 4, 4, 5, 2, 1, 2 });
    // Act
    Game.RollDice(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { One }));


    SetupDiceToRoll(new List<int> { 4, 4, 5, 2, 1 });

    Game.RollDice(new RollDice(1, 1));

    // Assert
    State.TableCenter!.Should().HaveCount(5);
  }
}