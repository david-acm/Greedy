using FluentAssertions;
using Xunit.Abstractions;
using static DiceGame.Commands;
using static DiceGame.GameEvents;

namespace DiceGame.Tests;

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
      .Should().HaveCount(1);
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
    playedOutOfTurn.Should().Be(
        new PlayedOutOfTurn(2, 1));
  }
}