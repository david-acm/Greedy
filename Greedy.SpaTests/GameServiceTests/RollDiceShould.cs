using System.Text.Json;
using System.Threading.Tasks;
using Greedy.Spa.Components;
using Greedy.Spa.Services;
using RichardSzalay.MockHttp;
using static Greedy.SpaTests.GameServiceTests.MockHttpClientBUnitHelpers;
using Die = Greedy.Spa.Services.Die;

namespace Greedy.SpaTests.GameServiceTests;

public class RollDiceShould {
  [Fact]
  public async Task ReturnDiceValues()
  {
    // Given
    var mock = GetMockHttpClient();
      mock.When("/diceRolls")
      .RespondJson(
        new CommandResponse(
          new(new[] { new Die("1", 1) }),
          true));

    var sut = new GameService(mock.ToHttpClient());

    // When
    var dice = await sut.RollDiceAsync(1, 1);

    // Then
    dice.Should().Contain(DiceValue.One);
  }
}