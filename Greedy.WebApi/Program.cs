using Eventuous;
using Eventuous.EventStore;
using Greedy.GameAggregate;
using Greedy.WebApi;
using Greedy.WebApi.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCommandService<GameService, Game>();
builder.Services.AddEventStoreClient("esdb://localhost:2113?tls=false");
builder.Services.AddAggregateStore<EsdbEventStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.UseHttpsRedirection();
app.UseAuthorization();
TypeMap.RegisterKnownEventTypes();
app
  .MapAggregateCommands<Game>()
  // .MapDiscoveredCommands<Game>()
  .MapCommand<V1.StartGameHttp, Command.StartGame>(
    (cmd, ctx) 
      => new Command.StartGame(
        cmd.Id))
  
  .MapCommand<V1.JoinPlayerHttp, Command.JoinPlayer>(
    (cmd, ctx) 
      => new Command.JoinPlayer(
        cmd.GameId,
        cmd.PlayerId,
        cmd.PlayerName))
  
  .MapCommand<V1.RollDiceHttp, Command.RollDice>(
    (cmd, ctx) 
      => new Command.RollDice(
        cmd.GameId,
        cmd.PlayerId))
  
  .MapCommand<V1.KeepDiceHttp, Command.KeepDice>(
    (cmd, ctx) 
      => new Command.KeepDice(
        cmd.GameId,
        cmd.PlayerId,
        cmd.DiceValues.ToDiceValues()))
  
  
  .MapCommand<V1.PassTurnHttp, Command.PassTurn>(
    (cmd, ctx) 
      => new Command.PassTurn(
        cmd.GameId,
        cmd.PlayerId))
  ;
app.Run();