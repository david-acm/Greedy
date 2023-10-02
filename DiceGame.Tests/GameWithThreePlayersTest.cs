using Moq;
using Xunit.Abstractions;

namespace DiceGame.Tests;

public class GameWithThreePlayersTest {
  protected readonly ITestOutputHelper Output;
  protected readonly Game Game;
  protected GameState State => Game.State;
  protected IReadOnlyList<object> Events => Game.Events;
  private readonly IRandom _randomProvider;
  private List<int>.Enumerator _enumerator;

  protected GameWithThreePlayersTest(ITestOutputHelper output) {
    Output = output;
    // Arrange
    _randomProvider = Mock.Of<IRandom>();
    Game = new Game(_randomProvider);

    // Act
    Game.Start(new GameId(1));
    Game.JoinPlayer(1, "David");
    Game.JoinPlayer(2, "Cristian");
    Game.JoinPlayer(3, "German");
  }


  protected void SetupDiceToThrow(List<int> _values) {
    _enumerator = _values.GetEnumerator();
    Mock.Get(_randomProvider).Setup(s => s.Next(It.IsAny<int>(), It.IsAny<int>())).Returns(() =>
    {
      _enumerator.MoveNext();
      return _enumerator.Current;
    });
  }

  public void Destruct() {
    _enumerator.Dispose();
  }
}