using System;

namespace Meta.Extensions;

public static class EventExtensions
{
    public static T RollRaise<T>(this Func<T, T> eventFn, T value)
    {
        if (eventFn == null)
            return value;

        foreach (Func<T, T> fn in eventFn.GetInvocationList())
            value = eventFn(value);

        return value;
    }
    public static T AggregateRaise<T>(this Func<T> eventFn, T value, Func<T, T, T> aggregateFn)
    {
        if (eventFn == null)
            return value;

        foreach (Func<T, T> fn in eventFn.GetInvocationList())
            value = aggregateFn(value, eventFn());

        return value;
    }
}
