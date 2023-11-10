namespace Greedy.Spa.Services;

public record CommandResponse(State State, bool Success);

public record State(Die[] TableCenter);

public record Die(string Name, int Value);