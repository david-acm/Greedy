using System.Collections.Immutable;
using Eventuous;
using static Greedy.GameAggregate.GameEvents;

namespace Greedy.GameAggregate;

public class Game : Aggregate<GameState> {
  private readonly IRandom _randomProvider;

  public Game() : this(default!)
  {
  }
  
  public Game(IRandom? randomProvider)
  {
    _randomProvider = randomProvider ?? new DefaultRandomProvider();
  }

  public void Start(Command.StartGame startGame) => Apply(new V1.GameStarted(startGame));

  public void JoinPlayer(Command.JoinPlayer joinPlayer) =>
    Apply(new V1.PlayerJoined(joinPlayer.Id, joinPlayer.Name));

  public void RollDiceV1(Command.RollDice rollDice)
  {
    var roll = Dice.FromNewRoll(
      _randomProvider,
      GetNumberOfDiceToTrow());

    Apply(new V1.DiceRolled(
      rollDice.PlayerId,
      roll.DiceValues.ToPrimitiveArray(),
      GetScoreAfterRoll(roll)));
  }

  public void RollDiceV2(Command.RollDice rollDice)
  {
    var roll = Dice.FromNewRoll(
      _randomProvider,
      GetNumberOfDiceToTrow());

    Apply(new V2.DiceRolled(
      rollDice.PlayerId,
      roll.DiceValues.ToPrimitiveArray(),
      GetScoreAfterRoll(roll),
      GameStage.Keeping));
  }

  public void PassTurn(Command.PassTurn passTurn) =>
    Apply(new V1.TurnPassed(
      passTurn.PlayerId,
      GetPlayerOrder(passTurn.PlayerId),
      GetScore(passTurn.PlayerId)));

  public void KeepDice(Command.KeepDice keepDice) =>
    Apply(new V1.DiceKept(keepDice.PlayerId, keepDice.DiceValues.ToPrimitiveArray(),
      GetTableCenterDice(keepDice),
      GetNewTurnScore(keepDice.DiceValues, State.TurnScore)));

  public void KeepDiceV2(Command.KeepDice keepDice) =>
    Apply(new V2.DiceKept(keepDice.PlayerId, keepDice.DiceValues.ToPrimitiveArray(),
      GetTableCenterDice(keepDice),
      GetNewTurnScore(keepDice.DiceValues, State.TurnScore),
      GameStage.Rolling));

  private int[] GetTableCenterDice(Command.KeepDice keepDice)
  {
    var tableCenter = State.TableCenter.RemoveRange(keepDice.DiceValues);
    if (!tableCenter.IsEmpty) return tableCenter.ToPrimitiveArray();

    return State.TableCenter.AddRange(keepDice.DiceValues).ToPrimitiveArray();
  }

  private int GetScore(Command.PlayerId playerId) =>
    State.GameScoreFor(playerId) + State.TurnScore;

  private ImmutableArray<Player> GetPlayerOrder(int playerId)
  {
    var player        = State.GetPlayer(playerId);
    var newPlayerList = State.Players.Remove(player).Add(player);
    return newPlayerList;
  }

  private void Apply(object @event)
  {
    try
    {
      GameValidator.EnsurePreconditions(
        this, @event);
      base.Apply(@event);
    }
    catch (PreconditionsFailedException e)
    {
      base.Apply(e.Event);
      throw;
    }
  }

  private int GetNumberOfDiceToTrow() => 6 - State.DiceKept.Length;

  private static int GetNewTurnScore(IEnumerable<DiceValue> diceKept, int currentScore)
  {
    var dice = new Dice(diceKept);
    var tricks = new Dictionary<Validator, int>
    {
      { new DiceAreStraight(dice), 1000 },
      { new DiceAreTrips(dice), dice.DiceValues.First().Value * 100 },
      {
        new DiceAreOnesOrFives(dice),
        dice.DiceValues.Count(d => d == DiceValue.One) * 100 + dice.DiceValues.Count(d => d == DiceValue.Five) * 50
      },
      { new DiceAreStair(dice), 1500 }
    };

    int turnScore = tricks.FirstOrDefault(v => v.Key.IsSatisfied()).Value;

    return new Score(currentScore + turnScore);
  }

  private Score GetScoreAfterRoll(Dice dice)
  {
    var canKeepAnyDice = new CanKeepDice(dice).IsSatisfied();

    return canKeepAnyDice ? State.TurnScore : new Score(0);
  }
}

public record Player(int Id, string Name);

public enum GameStage {
  None,
  Rolling,
  Keeping
}