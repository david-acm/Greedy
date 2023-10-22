using FluentAssertions;
using Moq;

namespace DiceGame.Tests;

// Dominio
// NombreClase_Should (Deberia)
public class Game1StartShould {
  [Fact]
  // CambiarEstadoAIniciado
  
  // El metodo start deberia cambiar el estado a iniciado
  public void ChangeStateToStarted() {
    // Arrange (Preparar) - Cuando While
    var game = new Game1();

    // Act (Ejecutar) - Como When
    game.Start(1);

    // Assert (Afirmar) - Entonces Then
    game.Stage.Should().Be(GameStage.Started);
  }
}

// Command -> Service (GameService) -> Entity (Game)
public class Start1Should {
  // Guarde en DB
  [Fact]
  public void SaveGameInDB() {
    // Arrange
    var eventStore     = Mock.Of<IEventStore>();
    var eventStoreMock = Mock.Get(eventStore);
    var service        = new GameService(eventStore);

    // Act
    service.StartGame(1);

    // Assert
    VerifyThatEventsPassedContainGameStarted(eventStoreMock);
  }
  
  [Fact]
  public void LoadGameInDB() {
    // Arrange
    var eventStore     = Mock.Of<IEventStore>();
    var eventStoreMock = Mock.Get(eventStore);
    var service        = new GameService(eventStore);

    // Act
    service.StartGame(1);
    // var game = service.JoinPlayer(1, "David");

    // Assert
    // game.Stage.Should().Be(GameStage.Started);
  }

  private static void VerifyThatEventsPassedContainGameStarted(Mock<IEventStore> eventStoreMock) =>
    eventStoreMock.Verify(e => e.Save(It.Is<object []>(e => e.FirstOrDefault() is GameStarted)), Times.Once);
}