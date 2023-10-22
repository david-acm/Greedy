using FluentAssertions;

namespace DiceGame.Tests;

public class JoinPlayerShould {

  [Fact]
  public void AddPlayerToGame() {
    // Arrange - While
    var game = new Game();
    game.Start(1);

    // Act - When
    game.JoinPlayer("Cristian");

    // Assert - Then
    game.Players.Should().Contain(new Player("Cristian"));
    game.Events.Should().Contain(new PlayerJoined("Cristian"));
  }
}