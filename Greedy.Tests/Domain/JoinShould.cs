using Greedy.GameAggregate;
using FluentAssertions;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.GameEvents.V1;

namespace Greedy.Tests.Domain;

public class JoinShould {
  [Fact]
  public void AddPlayerToGame() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(new StartGame(1));
    var player1 = new JoinPlayer(1, 1, "David");
    game.JoinPlayer(player1);
    game.JoinPlayer(new JoinPlayer(1, 2, "Cristian"));

    // Assert
    game.Changes.Where(p => p is PlayerJoined).Should().HaveCount(2);
    game.State.Players.Should().Contain(new Player(1, "David"));
  }
}