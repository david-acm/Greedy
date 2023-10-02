using FluentAssertions;
using Moq;
using Xunit.Abstractions;
using static DiceGame.DiceValue;
using static DiceGame.GameEvents;

namespace DiceGame.Tests;

public class ThrowShould : GameWithThreePlayersTest {
  public ThrowShould(ITestOutputHelper output) : base(output) {
  }

  [Fact]
  public void AllowPlayerToThrow() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(1);
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");
    game.JoinPlayer(3, "German");
    game.ThrowDice(1);
    game.Pass(1);

    // Assert
    game.State.Throws.Should().HaveCount(1);
    var diceThrown = game.Events.Where(e => e is DiceThrown)
      .Should().HaveCount(1).And.Subject;
    diceThrown.Should()
      .ContainSingle(e =>
        ((DiceThrown)e).PlayerId == 1);
  }

  [Fact]
  public void NotAllowPlayerToThrowOutOfTurn() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(1);
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");
    game.JoinPlayer(3, "German");
    var action = () => game.ThrowDice(2);

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    game.State.Throws.Should().HaveCount(0);
    var playedOutOfTurn = game.Events
      .Where(e => e is PlayedOutOfTurn).Should().ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((PlayedOutOfTurn)e).TriedToPlay == 2 &&
        ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void NotAllowNextPlayerToPlayUntilPlayerPasses() {
    // Arrange
    var game = new Game();

    // Act
    game.Start(1);
    game.JoinPlayer(1, "David");
    game.JoinPlayer(2, "Cristian");
    game.JoinPlayer(3, "German");
    game.ThrowDice(1);

    var action = () => game.ThrowDice(2);

    // Assert
    action.Should().Throw<PreconditionsFailedException>();
    game.State.Throws.Should().HaveCount(1);
    var playedOutOfTurn = game.Events
      .Where(e => e is PlayedOutOfTurn).Should().ContainSingle()
      .And.Subject;
    playedOutOfTurn.Should()
      .Satisfy(e =>
        ((PlayedOutOfTurn)e).TriedToPlay == 2 &&
        ((PlayedOutOfTurn)e).ExpectedPlayer == 1);
  }

  [Fact]
  public void ThrowOnlyAvailableDiceAtTheTableCenter() {
    // Arrange
    SetupDiceToThrow(new List<int>() { 4, 4, 5, 2, 1, 2, 3 });

    // Act
    Game.ThrowDice(1);
    Game.Keep(1, new[] { One });
    SetupDiceToThrow(new List<int>() { 4, 4, 5, 2, 1, 2 });
    Game.ThrowDice(1);
    Game.Keep(1, new[] { Five });

    // Assert
    State.Throws.Should().HaveCount(2);
    State.Throws.Last().Dice.DiceValues.Should().HaveCount(5);
  }
}

public class GameWithThreePlayersTest {
  protected readonly ITestOutputHelper Output;
  protected readonly Game Game;
  protected GameState State => Game.State;
  private readonly IRandom _randomProvider;
  private List<int>.Enumerator _enumerator;

  public GameWithThreePlayersTest(ITestOutputHelper output) {
    Output = output;
    // Arrange
    _randomProvider = Mock.Of<IRandom>();
    Game = new Game(_randomProvider);

    // Act
    Game.Start(1);
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