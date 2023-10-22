using Moq;

namespace DiceGame.Tests;

public class SaveShould {
  [Fact]
  public void SaveGameInDB() {
    // Arrange
    var eventStore = Mock.Of<IEventStore>();
    var mock       = Mock.Get(eventStore);
    var service    = new GameService(eventStore);

    // Act
    service.StartGame(1);

    // Assert
    VerifyThatEventsPassedContainGameStarted(mock);
  }

  private static void VerifyThatEventsPassedContainGameStarted(Mock<IEventStore> eventStoreMock) {
    eventStoreMock.Verify(e => e.Save(It.Is<object[]>(e => e.FirstOrDefault() is GameStarted)), Times.Once);
  }
}