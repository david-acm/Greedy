using FluentAssertions;
using Moq;
using Xunit.Abstractions;
using static DiceGame.DiceValue;
using static DiceGame.GameEvents;

namespace DiceGame.Tests;

public class KeepShould : GameWithThreePlayersTest {

  public KeepShould(ITestOutputHelper helper) 
  : base(helper){
  }

  [Fact]
  public void OnlyAllowToKeepByThePlayerInTurn() {
    // Arrange
    Game.ThrowDice(1);
    var action = () => Game.Keep(2, new[]
    {
      Five,
      One,
    });

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Game.Events.Should()
      .ContainSingleEvent<PlayedOutOfTurn>();
  }

  [Fact]
  public void OnlyAllowToKeepFivesAndOnes_WhenThePlayerDidntGetAnyOtherTricks() {
    // Arrange
    Game.ThrowDice(1);
    var action = () => Game.Keep(1, new[]
    {
      Four,
    });

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Game.Events.Should()
      .ContainSingleEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepTrips() {
    // Arrange
    var values = new List<int>()
    {
      4, 4, 4, 2, 1, 2, 3
    };
    SetupDiceToThrow(values);

    Game.ThrowDice(1);

    var action = () => Game.Keep(1, new[]
    {
      Four,
      Four,
      Four,
    });

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Game.Events.Should()
      .NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepStair() {
    // Arrange
    var values = new List<int>()
    {
      1, 2, 3, 4, 5, 6
    };
    SetupDiceToThrow(values);
    var action = () => Game.Keep(1, new[]
    {
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
    });

    //Act
    Game.ThrowDice(1);

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    Game.Events.Should()
      .NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepOnlyDiceThatWereThrown() {
    // Arrange
    Game.ThrowDice(1);
    var diceValues = new[]
    {
      One,
      Two,
      Three,
      Four,
      Five,
      Six,
    };

    var last = Game.State.Throws.Last().Dice.DiceValues;

    var diceToKeep = diceValues.Where(d => !last.Contains(d)).ToArray();

    var action = () => Game.Keep(1, diceToKeep);

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Game.Events.Should()
      .ContainSingleEvent<DiceNotAllowedToBeKept>();
  }


  [Fact]
  public void AllowToKeepOnlyDiceThatAreStillInTheTable() {
    // Arrange
    var values = new List<int>()
    {
      1, 1, 3, 4, 5, 6
    };
    SetupDiceToThrow(values);
    Game.ThrowDice(1);
    var diceValues = new[]
    {
      One,
    };

    var last = Game.State.Throws.Last().Dice.DiceValues;

    var diceToKeep = diceValues.First(d => last.Contains(d) && d == One);

    Game.Keep(1, new[] { diceToKeep });

    diceToKeep = Game.State.DiceKept.First();

    var action = () => Game.Keep(1, new[] { diceToKeep });

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Game.Events.Should()
      .NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void RemoveDiceFromTableCenter() {
    // Arrange
    SetupDiceToThrow(new List<int>() { 1, 2, 3, 4, 5, 6 });
    var diceToKeep = new[] { One, Five };

    //Act
    Game.ThrowDice(1);
    var action = () => Game.Keep(1, diceToKeep);

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    Game.State.TableCenter.Should().HaveCount(4);
  }
}