using FluentAssertions;
using FluentAssertions.Collections;

namespace Greedy.Tests.Framework;

public static class GenericCollectionAssertionsExtensions {
  public static TEvent? ContainSingleEvent<TEvent>(
    this GenericCollectionAssertions<object> assertion)
    where TEvent : class {
    var @event = assertion.Subject.Where(e => e is TEvent);
    @event.Should().ContainSingle();

    return @event.FirstOrDefault() as TEvent;
  }

  public static void NotContainAnyEvent<TEvent>(
    this GenericCollectionAssertions<object> assertion)
    where TEvent : class {
    var @event = assertion.Subject.Where(e => e is TEvent);
    @event.Should().BeEmpty();
  }
}