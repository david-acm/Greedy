using FluentAssertions;
using Greedy.GameAggregate;
using Greedy.Tests.Framework;
using Xunit.Abstractions;
using static Greedy.GameAggregate.GameEvents.V1;

namespace Greedy.Tests.Domain;

public class KeepDiceShould : GameWithThreePlayersTest {
  public KeepDiceShould(ITestOutputHelper helper)
    : base(helper)
  {
  }

  public static IEnumerable<object[]> KeepCommands()
  {
    yield return new[]
    {
      (Action<Game>)(g => g.KeepDice(new Command.KeepDice(1, 2, new[]
      {
        DiceValue.Five, DiceValue.One
      })))
    };
    yield return new[]
    {
      (Action<Game>)(g => g.KeepDiceV2(new Command.KeepDice(1, 2, new[]
      {
        DiceValue.Five, DiceValue.One
      })))
    };
  }

  [Theory]
  [MemberData(nameof(KeepCommands))]
  public void OnlyAllowToKeepByThePlayerInTurn(Action<Game> keepAction)
  {
    // Arrange
    Game.RollDiceV2(new Command.RollDice(1, 1));

    var action = () => keepAction(Game);

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Changes.Should().ContainSingleEvent<PlayedOutOfTurn>();
  }

  [Fact]
  public void OnlyAllowToKeepFivesAndOnes_WhenThePlayerDidntGetAnyOtherTricks()
  {
    // Arrange
    Game.RollDiceV2(new Command.RollDice(1, 1));
    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, new[]
    {
      DiceValue.Four
    }));

    //Act
    action.Should().Throw<PreconditionsFailedException>();
    Changes.Should().ContainSingleEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepTrips()
  {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 4, 4, 4, 2, 1, 2, 3 });
    Game.RollDiceV2(new Command.RollDice(1, 1));

    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, new[]
    {
      DiceValue.Four, DiceValue.Four, DiceValue.Four
    }));

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Changes.Should().NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepAStraight()
  {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 4, 4, 4, 4, 1, 2, 3 });
    Game.RollDiceV2(new Command.RollDice(1, 1));

    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, new[]
    {
      DiceValue.Four, DiceValue.Four, DiceValue.Four, DiceValue.Four
    }));

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Changes.Should().NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepStair()
  {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 1, 2, 3, 4, 5, 6 });
    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, new[]
    {
      DiceValue.One, DiceValue.Two, DiceValue.Three, DiceValue.Four, DiceValue.Five, DiceValue.Six
    }));

    //Act
    Game.RollDiceV2(new Command.RollDice(1, 1));

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    Changes.Should().NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepOnlyDiceThatWereRolled()
  {
    // Arrange
    Game.RollDiceV2(new Command.RollDice(1, 1));
    var diceValues = new[]
    {
      DiceValue.One, DiceValue.Two, DiceValue.Three, DiceValue.Four, DiceValue.Five, DiceValue.Six
    };

    var last           = State.TableCenter!;
    var diceToKeep     = diceValues.Where(d => !last.Contains(d));
    var keepDiceAction = () => Game.KeepDice(new Command.KeepDice(1, 1, diceToKeep));

    //Act
    keepDiceAction.Should().Throw<PreconditionsFailedException>();
    Changes.Should().ContainSingleEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void AllowToKeepOnlyDiceThatAreStillInTheTable()
  {
    // Arrange
    var values = new List<int>
    {
      1, 1, 3, 4, 5, 6
    };
    SetupDiceToRoll(values);
    Game.RollDiceV2(new Command.RollDice(1, 1));
    var diceValues = new[]
    {
      DiceValue.One
    };

    var tableCenter = State.TableCenter!;

    var diceToKeep = diceValues.First(d => tableCenter.Contains(d) && d == DiceValue.One);

    Game.KeepDice(new Command.KeepDice(1, 1, new[] { diceToKeep }));

    diceToKeep = State.DiceKept.First();

    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, new[] { diceToKeep }));

    //Act
    action.Should().NotThrow<PreconditionsFailedException>();
    Changes.Should().NotContainAnyEvent<DiceNotAllowedToBeKept>();
  }

  [Fact]
  public void RemoveDiceFromTableCenter()
  {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 1, 2, 3, 4, 5, 6 });
    var diceToKeep = new[] { DiceValue.One, DiceValue.Five };

    //Act
    Game.RollDiceV2(new Command.RollDice(1, 1));
    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, diceToKeep));

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
    int         expectedScore)
  {
    // Arrange
    SetupDiceToRoll(rolledDice);
    Game.RollDiceV2(new Command.RollDice(1, 1));
    var action = () => Game.KeepDice(new Command.KeepDice(1, 1, diceToKeep));

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    State.TurnScore.Should().Be(new Score(expectedScore),
      $"{reason} but got {State.TurnScore}");
  }

  [Fact]
  public void ResetScoreIfPlayerGetsNoTricks()
  {
    // Arrange
    SetupDiceToRoll(new List<int>
      { 1, 2, 3, 4, 5, 6 });
    Game.RollDiceV2(new Command.RollDice(1, 1));
    Game.KeepDice(new Command.KeepDice(1, 1, new[] { DiceValue.One }));

    SetupDiceToRoll(new List<int>
      { 2, 2, 3, 3, 4, 6 });
    // Act
    Game.RollDiceV2(new Command.RollDice(1, 1));

    // Assert
    State.TurnScore.Should().Be(new Score(0));
  }

  [Fact]
  public void AddToTurnScore()
  {
    // Arrange
    SetupDiceToRoll(new List<int> { 1, 2, 3, 4, 5, 6 });
    Game.RollDiceV2(new Command.RollDice(1, 1));
    Game.KeepDice(new Command.KeepDice(1, 1, new[] { DiceValue.One }));

    SetupDiceToRoll(new List<int> { 1, 1, 3, 3, 4 });
    Game.RollDiceV2(new Command.RollDice(1, 1));
    Game.KeepDice(new Command.KeepDice(1, 1, new[] { DiceValue.One }));

    // Assert
    State.TurnScore.Should().Be(new Score(200));
  }

  [Fact]
  public void ResetDiceInTableCenterWhenAllDiceHaveBeenKept()
  {
    // Arrange
    SetupDiceToRoll(new List<int> { 1, 1, 1, 2, 3, 4 });
    Game.RollDiceV2(new Command.RollDice(1, 1));
    Game.KeepDice(new Command.KeepDice(1, 1, new[] { DiceValue.One, DiceValue.One, DiceValue.One }));

    SetupDiceToRoll(new List<int> { 4, 4, 4 });
    Game.RollDiceV2(new Command.RollDice(1, 1));
    Game.KeepDice(new Command.KeepDice(1, 1, new[] { DiceValue.Four, DiceValue.Four, DiceValue.Four }));

    // Assert
    State.TableCenter.Should().HaveCount(6);
  }

  public static IEnumerable<object[]> TricksAndScore()
  {
    yield return new object[]
    {
      "1 should add 100",
      new[] { 1, 2, 2, 3, 4, 4 }, new[] { DiceValue.One }, 100
    };
    yield return new object[]
    {
      "1 and 5 should add 150",
      new[] { 1, 1, 2, 3, 4, 5 }, new[] { DiceValue.One, DiceValue.Five, DiceValue.One }, 250
    };
    yield return new object[]
    {
      "2, 2, 2 should add 200",
      new[] { 3, 3, 3, 3, 4, 4 }, new[] { DiceValue.Three, DiceValue.Three, DiceValue.Three }, 300
    };
    yield return new object[]
    {
      "4, 4, 4, 4 should add 1000",
      new[] { 3, 3, 4, 4, 4, 4 }, new[] { DiceValue.Four, DiceValue.Four, DiceValue.Four, DiceValue.Four }, 1000
    };
  }
}