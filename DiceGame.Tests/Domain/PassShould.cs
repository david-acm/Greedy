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
    Game.ThrowDice(new PlayerId(1));
    Game.Pass(new PlayerId(1));

    // Assert
    Events.Where(e => e is TurnPassed)
      .Should()
      .HaveCount(1);
  }

  [Fact]
  public void NotAllowPlayerNotInTurnToPass() {
    // Act
    Game.ThrowDice(new PlayerId(1));
    var action = () => Game.Pass(new PlayerId(2));

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    State.Throws.Should().HaveCount(1);
    var playedOutOfTurn = Events.Should().ContainSingleEvent<PlayedOutOfTurn>();
    playedOutOfTurn.Should()
      .Be(
        new PlayedOutOfTurn(2, 1));
  }

  [Fact]
  public void AddScoreToPlayer() {
    // Arrange
    SetupDiceToThrow(new List<int>() { 1, 2, 3, 4, 5, 6 });
    
    // Act
    Game.ThrowDice(new PlayerId(1));
    Game.Keep(new Keep(1, new[] { One }));
    var action = () => Game.Pass(new PlayerId(1));

    // Assert
    action.Should().NotThrow<PreconditionsFailedException>();
    State.ScoreFor(new PlayerId(1)).Should().Be(new Score(new PlayerId(1), 100));
  }
}