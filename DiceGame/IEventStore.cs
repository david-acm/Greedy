namespace DiceGame;

public interface IEventStore {
  void     Save(object[] gameStarted);
  object[] Load<T>(int   id);
}