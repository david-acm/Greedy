using FluentAssertions;
using Moq;

namespace DiceGame.Tests;

public class LoadShould {
  [Fact]
  public void LoadGameFromDB() {
    // Arrange
    var mock    = new Mock<IEventStore>();
    var service = new GameService(mock.Object);

    // Act
    service.StartGame(1);
    // ------ new request
    var game = service.JoinGame(1, "Cristian");

    // Assert
    game.Players.Should().Contain(new Player("Cristian"));
    game.Stage.Should().Be(GameStage.Started);
  }

  private static void VerifyThatEventsPassedContainGameStarted(Mock<IEventStore> eventStoreMock) {
    eventStoreMock.Verify(e => e.Save(It.Is<object[]>(e => e.FirstOrDefault() is GameStarted)), Times.Once);
  }
}