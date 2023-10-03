using DiceGame.GameAggregate;
using DiceGame.Tests.Framework;
using FluentAssertions;
using Xunit.Abstractions;
using static DiceGame.GameAggregate.Commands;
using static DiceGame.GameAggregate.DiceValue;
using static DiceGame.GameAggregate.GameEvents;

namespace DiceGame.Tests.Domain;

public class PassShould : GameWithThreePlayersTest {
  public PassShould(ITestOutputHelper output) : base(output) {
  }

  [Fact]
  public void AllowPlayerToPass() {
    // Act
    Game.RollDice(new PlayerId(1));
    Game.Pass(new PlayerId(1));

    // Assert
    Events.Where(e => e is TurnPassed)
      .Should()
      .HaveCount(1);
  }

  [Fact]
  public void NotAllowPlayerNotInTurnToPass() {
    // Act
    Game.RollDice(new PlayerId(1));
    var action = () => Game.Pass(new PlayerId(2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Rolls.Should().HaveCount(1);
    var playedOutOfTurn = Events.Should().ContainSingleEvent<PlayedOutOfTurn>();
    playedOutOfTurn.Should()
      .Be(
        new PlayedOutOfTurn(2, 1));
  }
  
  [Fact]
  public void AddToGameScoreIfPlayerKeptTricks() {
    // Arrange
    SetupDiceToRoll(new List<int> { 1, 2, 3, 4, 5, 6 });
    Game.RollDice(new PlayerId(1));
    Game.Keep(new Keep(1, new[] { One }));
    
    SetupDiceToRoll(new List<int> { 2, 2, 2, 4, 5, 6 });
    Game.RollDice(new PlayerId(1));
    Game.Keep(new Keep(1, new[] { Two, Two, Two }));
    Game.Pass(1);
    
    // Assert
    var score = State.GameScoreFor(new PlayerId(1));
    score.Should()
      .Be(new Score(300));
  }
}