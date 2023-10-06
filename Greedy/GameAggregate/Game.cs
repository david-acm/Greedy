using System.Collections.Immutable;
using Eventuous;
using static Greedy.GameAggregate.Command;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.GameAggregate;

public class Game : Aggregate<GameState> {
  private readonly IRandom _randomProvider;

  public Game() : this(default!) {
  }

  public Game(IRandom? randomProvider) {
    _randomProvider = randomProvider ?? new DefaultRandomProvider();
  }

  public void Start(StartGame startGame) => Apply(new GameStarted(startGame));

  public void JoinPlayer(JoinPlayer joinPlayer) =>
    Apply(new PlayerJoined(joinPlayer.Id, joinPlayer.Name));

  public void RollDice(RollDice rollDice) {
    var roll = Dice.FromNewRoll(
      _randomProvider,
      GetNumberOfDiceToTrow());

    Apply(new DiceRolled(
      rollDice.PlayerId,
      roll.DiceValues.ToPrimitiveArray(),
      GetScoreAfterRoll(roll)));
  }

  public void PassTurn(PassTurn passTurn) {
    Apply(new TurnPassed(
      passTurn.PlayerId,
      GetPlayerOrder(passTurn.PlayerId),
      GetScore(passTurn.PlayerId)));
  }

  public void KeepDice(KeepDice keepDice) =>
    Apply(new DiceKept(keepDice.PlayerId, keepDice.DiceValues.ToPrimitiveArray(),
      GetTableCenterDice(keepDice),
      GetNewTurnScore(keepDice.DiceValues, State.TurnScore)));

  private int[] GetTableCenterDice(KeepDice keepDice) {
    var tableCenter = State.TableCenter
      .RemoveRange(keepDice.DiceValues);
    if (!tableCenter.IsEmpty)
    {
      return tableCenter.ToPrimitiveArray();
    }
    
    return State.TableCenter.AddRange(keepDice.DiceValues).ToPrimitiveArray();
  }

  private int GetScore(PlayerId playerId) {
    return State.GameScoreFor(playerId) + State.TurnScore;
  }

  private ImmutableArray<Player> GetPlayerOrder(int playerId) {
    var player        = State.GetPlayer(playerId);
    var newPlayerList = State.Players.Remove(player).Add(player);
    return newPlayerList;
  }

  private void Apply(object @event) {
    try
    {
      GameValidator.EnsurePreconditions(
        State, @event);
      base.Apply(@event);
    }
    catch (PreconditionsFailedException e)
    {
      AddChange(e.Event);
      throw;
    }
  }

  private int GetNumberOfDiceToTrow() =>
    State.IsFirstRoll ? 6 : 6 - State.DiceKept.Length;

  private static int GetNewTurnScore(IEnumerable<DiceValue> diceKept, int currentScore) {
    var dice = new Dice(diceKept);
    var tricks = new Dictionary<Validator, int>
    {
      { new DiceAreTrips(dice), dice.DiceValues.First().Value * 100 },
      {
        new DiceAreOnesOrFives(dice),
        dice.DiceValues.Count(d => d == DiceValue.One) * 100 + dice.DiceValues.Count(d => d == DiceValue.Five) * 50
      },
      { new DiceAreStair(dice), 1500 }
    };

    var turnScore = tricks.FirstOrDefault(v => v.Key.IsSatisfied()).Value;

    return new Score(currentScore + turnScore);
  }

  private Score GetScoreAfterRoll(Dice dice) {
    var canKeepAnyDice = new CanKeepDice(dice).IsSatisfied();

    return canKeepAnyDice ? State.TurnScore : new Score(0);
  }
}

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Started
}