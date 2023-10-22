using FluentAssertions;

namespace DiceGame.Tests;

// Dominio (Entity)
public class StartShould {
  [Fact]
  public void ChangeGameStageToStarted() {
    // Arrange - While
    var game = new Game();

    // Act - When
    game.Start(1);

    // Assert - Then
    game.Stage.Should().Be(GameStage.Started);
  }
}

// Servicio -> Entity