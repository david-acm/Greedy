using System.Net.Http.Json;
using FluentAssertions;

namespace Greedy.WebTests;

public static class HttpClientExtensions {
  public static async Task<HttpResponseMessage> PostAndEnsureOkStatusCode(this HttpClient client, string route,
    object                                                                                body)
  {
    var    result  = await client.PostAsJsonAsync(route, body);
    string content = await result.Content.ReadAsStringAsync();

    result.IsSuccessStatusCode.Should().
      BeTrue($"Status code returned was: {result.StatusCode}, with reason: {result.ReasonPhrase} {content}");

    return result;
  }
}