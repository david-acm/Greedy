using DiceGame.GameAggregate;
using Moq;
using Xunit.Abstractions;
using static DiceGame.GameAggregate.Commands;

namespace DiceGame.Tests;

public class GameWithThreePlayersTest {
  private readonly   IRandom              _randomProvider;
  protected readonly Game                 Game;
  protected readonly ITestOutputHelper    Output;
  private            List<int>.Enumerator _enumerator;

  protected GameWithThreePlayersTest(ITestOutputHelper output) {
    Output = output;
    // Arrange
    _randomProvider = Mock.Of<IRandom>();
    SetupDiceToThrow(new List<int>
      { 4, 4, 4, 2, 1, 2, 3 });
    var game = new Game(_randomProvider);

    // Act
    game.Start(new StartGame(1));
    game.JoinPlayer(new JoinPlayer(1, "David"));
    game.JoinPlayer(new JoinPlayer(2, "Cristian"));
    game.JoinPlayer(new JoinPlayer(3, "German"));

    Game = new Game(_randomProvider);
    Game.Load(game.Events.ToList());
  }

  protected GameState             State  => Game.State;
  protected IReadOnlyList<object> Events => Game.Events;


  protected void SetupDiceToThrow(List<int> values) {
    _enumerator = values.GetEnumerator();
    Mock.Get(_randomProvider)
      .Setup(s => s.Next(It.IsAny<int>(), It.IsAny<int>()))
      .Returns(() =>
      {
        _enumerator.MoveNext();
        return _enumerator.Current;
      });
  }
}