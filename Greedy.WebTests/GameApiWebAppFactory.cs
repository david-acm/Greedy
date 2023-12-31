using DotNet.Testcontainers.Builders;
using Greedy.WebApi.Application;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Greedy.WebTests;

public class GameApiWebAppFactory : WebApplicationFactory<GameService> {
  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    var environementVariables = new Dictionary<string, string>
    {
      { "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true" },
      { "EVENTSTORE_INSECURE", "true" },
      { "EVENTSTORE_CLUSTER_SIZE", "1" },
      { "EVENTSTORE_EXT_TCP_PORT", "1113" },
      { "EVENTSTORE_HTTP_PORT", "2113" },
      { "EVENTSTORE_ENABLE_EXTERNAL_TCP", "true" },
      { "EVENTSTORE_RUN_PROJECTIONS", "all" },
      { "EVENTSTORE_START_STANDARD_PROJECTIONS", "true" },
      { "PATH", "/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin" },
      { "ASPNETCORE_URLS", "http://+:80" },
      { "DOTNET_RUNNING_IN_CONTAINER", "true" }
    };
    new ContainerBuilder()
      .WithImage("ghcr.io/eventstore/eventstore:21.10.0-alpha-arm64v8")
      .WithPortBinding(1113)
      .WithPortBinding(2113)
      .WithEnvironment(environementVariables)
      .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(2113)))
      .WithAutoRemove(false).Build()
      .StartAsync()
      .GetAwaiter()
      .GetResult();

    base.ConfigureWebHost(builder);
  }
}