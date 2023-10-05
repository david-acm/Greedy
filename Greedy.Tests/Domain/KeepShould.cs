using Greedy.GameAggregate;
using Greedy.Tests.Framework;
using FluentAssertions;
using Xunit.Abstractions;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.DiceValue;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.Tests.Domain;

public class KeepShould : GameWithThreePlayersTest {
  public KeepShould(ITestOutputHelper helper)
    : base(helper) {
  }

  [Fact]
  public void OnlyAllowToKeepByThePlayerInTurn() {
    // Arrange
    Game.RollDice(new RollDice(1, 1));
    var action = () => Game.Keep(new KeepDice(1, 2, new[]
    {
      Five,
      One
    }));

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Events.Should()
      .ContainSingleEvent<PlayedOutOfTurn>();
  }

  [Fact]
  public void OnlyAllowToKeepFivesAndOnes_WhenThePlayerDidntGetAnyOtherTricks() {
    // Arrange
    Game.RollDice(new RollDice(1, 1));
    var action = () => Game.Keep(new KeepDice(1, 1, new[]
    {
      Four
    }));

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Events.Should()
      .ContainSingleEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepTrips() {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 4, 4, 4, 2, 1, 2, 3 });
    Game.RollDice(new RollDice(1, 1));

    var action = () => Game.Keep(new KeepDice(1, 1, new[]
    {
      Four,
      Four,
      Four
    }));

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Events.Should()
      .NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepStair() {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 1, 2, 3, 4, 5, 6 });
    var action = () => Game.Keep(new KeepDice(1, 1, new[]
    {
      One,
      Two,
      Three,
      Four,
      Five,
      Six
    }));

    //Act
    Game.RollDice(new RollDice(1, 1));

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    Events.Should()
      .NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepOnlyDiceThatWereRolled() {
    // Arrange
    Game.RollDice(new RollDice(1, 1));
    var diceValues = new[]
    {
      One,
      Two,
      Three,
      Four,
      Five,
      Six
    };

    var last = State.LastRoll!.Dice.DiceValues;

    var diceToKeep = diceValues.Where(d => !last.Contains(d));

    var action = () => Game.Keep(new KeepDice(1, 1, diceToKeep));

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Events.Should()
      .ContainSingleEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepOnlyDiceThatAreStillInTheTable() {
    // Arrange
    var values = new List<int>
    {
      1, 1, 3, 4, 5, 6
    };
    SetupDiceToRoll(values);
    Game.RollDice(new RollDice(1, 1));
    var diceValues = new[]
    {
      One
    };

    var last = State.LastRoll!.Dice.DiceValues;

    var diceToKeep = diceValues.First(d => last.Contains(d) && d == One);

    Game.Keep(new KeepDice(1, 1, new[] { diceToKeep }));

    diceToKeep = State.DiceKept.First();

    var action = () => Game.Keep(new KeepDice(1, 1, new[] { diceToKeep }));

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Events.Should()
      .NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void RemoveDiceFromTableCenter() {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 1, 2, 3, 4, 5, 6 });
    var diceToKeep = new[] { One, Five };

    //Act
    Game.RollDice(new RollDice(1, 1));
    var action = () => Game.Keep(new KeepDice(1, 1, diceToKeep));

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    State.TableCenter.Should().HaveCount(4);
  }

  [Theory]
  [MemberData(nameof(TricksAndScore))]
  public void AddTurnScoreToPlayer(
    string      reason,
    int[]       rolledDice,
    DiceValue[] diceToKeep,
    int         expectedScore) {
    // Arrange
    SetupDiceToRoll(rolledDice);
    Game.RollDice(new RollDice(1, 1));
    var action = () => Game.Keep(new KeepDice(1, 1, diceToKeep));

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    State.TurnScore.Should()
      .Be(new Score(expectedScore),
        $"{reason} but got {State.TurnScore}");
  }

  [Fact]
  public void ResetScoreIfPlayerGetsNoTricks() {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 1, 2, 3, 4, 5, 6 });
    Game.RollDice(new RollDice(1, 1));
    Game.Keep(new KeepDice(1, 1, new[] { One }));
    
    SetupDiceToRoll(new List<int>
      { 2, 2, 3, 3, 4, 6 });
    Game.RollDice(new RollDice(1, 1));
    
    // Assert
    State.TurnScore.Should()
      .Be(new Score(0));
  }
  
  [Fact]
  public void AddToTurnScore() {
    // Arrange
    SetupDiceToRoll(new List<int> { 1, 2, 3, 4, 5, 6 });
    Game.RollDice(new RollDice(1, 1));
    Game.Keep(new KeepDice(1, 1, new[] { One }));
    
    SetupDiceToRoll(new List<int> { 1, 1, 3, 3, 4 });
    Game.RollDice(new RollDice(1, 1));
    Game.Keep(new KeepDice(1, 1, new[] { One }));
    
    // Assert
    State.TurnScore.Should()
      .Be(new Score(200));
  }

  public static IEnumerable<object[]> TricksAndScore() {
    yield return new object[]
    {
      "1 should add 100",
      new[] { 1, 2, 2, 3, 4, 4 }, new[] { One }, 100
    };
    yield return new object[]
    {
      "1 and 5 should add 150",
      new[] { 1, 1, 2, 3, 4, 5 }, new[] { One, Five, One }, 250
    };
    yield return new object[]
    {
      "2, 2, 2 should add 200",
      new[] { 3, 3, 3, 3, 4, 4 }, new[] { Three, Three, Three }, 300
    };
  }
}