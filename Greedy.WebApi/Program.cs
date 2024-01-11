using Azure.Identity;
using Azure.Messaging.EventGrid;
using Azure.Messaging.ServiceBus;
using Eventuous;
using Eventuous.EventStore;
using Greedy.GameAggregate;
using Greedy.WebApi;
using Greedy.WebApi.Application;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration.Extensions;

const string MyAllowSpecificOrigins = "MyAllowSpecificOrigins";
const string esdbConnectionString   = "esdb://localhost:2113?tls=false";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCommandService<GameService, Game>();
builder.Services.AddEventStoreClient(esdbConnectionString);
builder.Services.AddAggregateStore<EsdbEventStore>();

IConfigurationRefresher refresher = default!;

var configuration     = builder.Configuration;
var appConfigEndpoint = configuration.GetValue<string>("AppConfigEndpoint");

builder.Configuration.AddAzureAppConfiguration(options =>
  {
    options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential());
    options.Select(KeyFilter.Any, "local");
    options.ConfigureRefresh(refresh =>
    {
      refresh.SetCacheExpiration(TimeSpan.FromDays(1));
      refresh.Register("Sentinel", refreshAll: true);
    });
    options.ConfigureKeyVault(kv =>
    {
      kv.SetCredential(new DefaultAzureCredential());
    });
    refresher = options.GetRefresher();
  });

await RegisterRefreshEventHandlerAsync(configuration, refresher);

builder.Services.AddCors(options =>
{
  options.AddPolicy(MyAllowSpecificOrigins,
    policy => { policy.WithOrigins("http://localhost:5186").AllowAnyHeader().AllowAnyMethod(); });
});

var app = builder.Build();

var logger = app.Services.GetService<ILogger<Program>>();
logger?.LogInformation($"Using configuration sentinel version: {configuration["Sentinel"]}");
logger?.LogInformation($"Using service bus connection: {configuration["ConnectionStrings:ServiceBus"]}");

 app.UseCors(MyAllowSpecificOrigins);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

// app.UseAzureAppConfiguration();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

// app.UseHttpsRedirection();
app.UseAuthorization();
TypeMap.RegisterKnownEventTypes();
app.MapAggregateCommands<Game>()
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
return;

async Task RegisterRefreshEventHandlerAsync(IConfiguration config, IConfigurationRefresher configRefresher) {
  await refresher.TryRefreshAsync();
  
  var serviceBusConnectionString = config.GetConnectionString("ServiceBus");
  var serviceBusQueue            = config.GetValue<string>("ServiceBusQueue");
  var serviceBusClient           = new ServiceBusClient(serviceBusConnectionString);
  var serviceBusProcessor        = serviceBusClient.CreateProcessor(serviceBusQueue);

  serviceBusProcessor.ProcessMessageAsync += (processMessageEventArgs) =>
  {
    // Build EventGridEvent from notification message
    var eventGridEvent = EventGridEvent.Parse(processMessageEventArgs.Message.Body);
    // Create PushNotification from eventGridEvent
    eventGridEvent.TryCreatePushNotification(out var pushNotification);

    // Prompt Configuration Refresh based on the PushNotification
    configRefresher.ProcessPushNotification(pushNotification);

    return Task.CompletedTask;
  };

  serviceBusProcessor.ProcessErrorAsync += (exceptionargs) =>
  {
    Console.WriteLine($"{exceptionargs.Exception}");
    return Task.CompletedTask;
  };

  await serviceBusProcessor.StartProcessingAsync();
}