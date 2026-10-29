using AnimeTracker.SharedKernel;
using Shouldly;

namespace AnimeTracker.SharedKernel.UnitTests;

public sealed class EntityTests
{
    private sealed class StubEntity : Entity<int>
    {
        public StubEntity(int id) { Id = id; }
    }

    [Fact]
    public void Entities_with_same_id_are_equal()
    {
        var a = new StubEntity(1);
        var b = new StubEntity(1);
        a.Equals(b).ShouldBeTrue();
        (a == b).ShouldBeFalse(); // reference equality is separate
    }

    [Fact]
    public void Entities_with_different_ids_are_not_equal()
    {
        var a = new StubEntity(1);
        var b = new StubEntity(2);
        a.Equals(b).ShouldBeFalse();
    }

    [Fact]
    public void Equal_entities_have_same_hash_code()
    {
        new StubEntity(42).GetHashCode()
            .ShouldBe(new StubEntity(42).GetHashCode());
    }

    [Fact]
    public void Equals_returns_false_for_null()
    {
        new StubEntity(1).Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void Equals_returns_false_for_different_type()
    {
        new StubEntity(1).Equals("not-an-entity").ShouldBeFalse();
    }
}

public sealed class AggregateRootTests
{
    private sealed class StubDomainEvent : IDomainEvent;

    private sealed class StubAggregate : AggregateRoot<int>
    {
        public StubAggregate(int id) { Id = id; }
        public void Publish(IDomainEvent e) => RaiseDomainEvent(e);
    }

    [Fact]
    public void RaiseDomainEvent_appends_event()
    {
        var agg = new StubAggregate(1);
        var evt = new StubDomainEvent();

        agg.Publish(evt);

        agg.DomainEvents.Count.ShouldBe(1);
        agg.DomainEvents[0].ShouldBeSameAs(evt);
    }

    [Fact]
    public void ClearDomainEvents_empties_the_list()
    {
        var agg = new StubAggregate(1);
        agg.Publish(new StubDomainEvent());

        agg.ClearDomainEvents();

        agg.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Multiple_events_are_preserved_in_order()
    {
        var agg = new StubAggregate(1);
        var e1 = new StubDomainEvent();
        var e2 = new StubDomainEvent();

        agg.Publish(e1);
        agg.Publish(e2);

        agg.DomainEvents.ShouldBe([e1, e2]);
    }
}
