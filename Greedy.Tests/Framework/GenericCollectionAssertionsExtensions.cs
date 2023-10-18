using FluentAssertions;
using FluentAssertions.Collections;

namespace Greedy.Tests.Framework;

public static class GenericCollectionAssertionsExtensions {
  public static TEvent? ContainSingleEvent<TEvent>(
    this GenericCollectionAssertions<object> assertion)
    where TEvent : class {
    var assertionSubject = assertion.Subject;
    var events           = assertionSubject.ToList();
    var @event = events.Should()
      .ContainSingle(e => e is TEvent, because:
        $"the items found were {string.Join(", ", events.Select(e => e.GetType().Name))}");

    return @event.Subject as TEvent;
  }

  public static IEnumerable<TEvent> ContainManyEvents<TEvent>(
    this GenericCollectionAssertions<object> assertion, int count)
    where TEvent : class {
    var assertionSubject = assertion.Subject;
    var events           = assertionSubject.ToList();
    var filteredEvents   = events.Where(e => e is TEvent).ToList();
    filteredEvents.Should()
      .HaveCount(count, because:
        $"the items found were {string.Join(", ", filteredEvents.Select(e => e.GetType().Name))}");

    return filteredEvents.Select(f => (TEvent)f);
  }

  public static void NotContainAnyEvent<TEvent>(
    this GenericCollectionAssertions<object> assertion)
    where TEvent : class {
    var @event = assertion.Subject.Where(e => e is TEvent);
    @event.Should().BeEmpty();
  }
}