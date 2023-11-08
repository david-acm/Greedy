namespace Greedy.Spa.Pages;

public record CommandResponse(State State, bool Success);
public record State(TableCenter[]   TableCenter);

public record TableCenter(string Name, int Value);