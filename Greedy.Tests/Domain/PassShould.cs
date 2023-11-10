using FluentAssertions;
using Greedy.GameAggregate;
using Greedy.Tests.Framework;
using Xunit.Abstractions;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.DiceValue;
using static Greedy.GameAggregate.GameEvents.V1;

namespace Greedy.Tests.Domain;

public class PassShould : GameWithThreePlayersTest {
  public PassShould(ITestOutputHelper output) : base(output)
  {
  }

  [Fact]
  public void AllowPlayerToPass()
  {
    // Act
    Game.RollDiceV2(new RollDice(1, 1));
    Game.PassTurn(new PassTurn(1, 1));

    // Assert
    Changes.Where(e => e is TurnPassed).Should().HaveCount(1);
  }

  [Fact]
  public void NotAllowPlayerNotInTurnToPass()
  {
    // Act
    Game.RollDiceV2(new RollDice(1, 1));
    var action = () => Game.PassTurn(new PassTurn(1, 2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    var playedOutOfTurn = Changes.Should().ContainSingleEvent<PlayedOutOfTurn>();
    playedOutOfTurn.Should().Be(
      new PlayedOutOfTurn(2, 1));
  }

  [Fact]
  public void NotAllowPlayerToPassWithoutRolling()
  {
    // Act
    Game.RollDiceV2(new RollDice(1, 1));
    Game.PassTurn(new PassTurn(1, 1));
    SetupDiceToRoll(new List<int> { 4, 4, 4, 2, 1, 2, 3 });
    var passOutOfTurn = () => Game.RollDiceV2(new RollDice(1, 1));
    var playOutOfTurn = () => Game.PassTurn(new PassTurn(1, 1));
    var passTurn      = () => Game.PassTurn(new PassTurn(1, 2));

    // Assert
    passOutOfTurn.Should().Throw<PreconditionsFailedException>();
    playOutOfTurn.Should().Throw<PreconditionsFailedException>();
    passTurn.Should().Throw<PreconditionsFailedException>();
    var passedWithoutRolling = Changes.Should().ContainSingleEvent<PassedWithoutRolling>();
    passedWithoutRolling.Should().Be(new PassedWithoutRolling(2));
  }

  [Fact]
  public void AddToGameScoreIfPlayerKeptTricks()
  {
    // Arrange
    SetupDiceToRoll(new List<int> { 1, 2, 3, 4, 5, 6 });
    Game.RollDiceV2(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { One }));

    SetupDiceToRoll(new List<int> { 2, 2, 2, 4, 5, 6 });
    Game.RollDiceV2(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { Two, Two, Two }));
    Game.PassTurn(new PassTurn(1, 1));

    // Assert
    var score = State.GameScoreFor(new PlayerId(1));
    score.Should().Be(new Score(300));
  }

  [Fact]
  public void ResetDiceInTableCenter()
  {
    // Arrange
    SetupDiceToRoll(new List<int> { 1, 5, 1, 4, 5, 6 });
    Game.RollDiceV2(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { One, Five, One }));

    SetupDiceToRoll(new List<int> { 1, 1, 4 });
    Game.RollDiceV2(new RollDice(1, 1));
    Game.KeepDice(new KeepDice(1, 1, new[] { One, One }));
    Game.PassTurn(new PassTurn(1, 1));

    // Assert
    var score = State.GameScoreFor(new PlayerId(1));
    score.Should().Be(new Score(450));
    State.TableCenter.Should().HaveCount(6);
  }
}