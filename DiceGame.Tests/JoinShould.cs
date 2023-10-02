using FluentAssertions;
using static DiceGame.GameEvents;

namespace DiceGame.Tests;

public class JoinShould {
  [Fact]
  public void AddPlayerToGame() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new GameId(1));
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");

    // Assert
    game.Events.Where(p => p is PlayerJoined).Should().HaveCount(2);
    game.State.Players.Should().Contain(new Player(1, "David"));
  }
}