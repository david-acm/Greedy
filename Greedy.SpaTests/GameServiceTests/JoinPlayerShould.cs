using System.Net.Http;
using System.Threading.Tasks;
using Greedy.Spa.Services;
using RichardSzalay.MockHttp;

namespace Greedy.SpaTests.GameServiceTests;

public class JoinPlayerShould {
  [Fact]
  public async Task CallApi()
  {
    // Given
    var mock = MockHttpClientBUnitHelpers.GetMockHttpClient();
    mock.Expect(HttpMethod.Post, "/players")
      .RespondJson(
        new CommandResponse(
          new(new[] { new Die("1", 1) }),
          true));

    var sut = new GameService(mock.ToHttpClient());

    // When
    await sut.JoinPlayerAsync(1, 1, "David");

    // Then
    mock.VerifyNoOutstandingExpectation();
  }
}