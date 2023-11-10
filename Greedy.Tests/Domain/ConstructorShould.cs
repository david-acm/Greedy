using FluentAssertions;
using Greedy.GameAggregate;

namespace Greedy.Tests.Domain;

public class ConstructorShould {
  [Fact]
  public void FallBackToDefaultRandomProviderWhenNoneIsInjected()
  {
    // Arrange
    var sut = () => new Game();

    // Act
    sut.Should().NotThrow();
  }
}