using System;
using System.Diagnostics;
using System.Threading.Tasks;

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

    public static async void RaiseNoWaitAsync(this Func<Task> onEvent)
    {
        if (onEvent != null)
        {
            foreach (Func<Task> action in onEvent.GetInvocationList())
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }

    public static async void RaiseNoWaitAsync<T1>(this Func<T1, Task> onEvent, T1 arg1)
    {
        if (onEvent != null)
        {
            foreach (Func<T1, Task> action in onEvent.GetInvocationList())
            {
                try
                {
                    await action(arg1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }

    public static async void RaiseNoWaitAsync<T1, T2>(this Func<T1, T2, Task> onEvent, T1 arg1, T2 arg2)
    {
        if (onEvent != null)
        {
            foreach (Func<T1, T2, Task> action in onEvent.GetInvocationList())
            {
                try
                {
                    await action(arg1, arg2);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }

    public static async void RaiseNoWaitAsync<T1, T2, T3>(this Func<T1, T2, T3, Task> onEvent, T1 arg1, T2 arg2, T3 arg3)
    {
        if (onEvent != null)
        {
            foreach (Func<T1, T2, T3, Task> action in onEvent.GetInvocationList())
            {
                try
                {
                    await action(arg1, arg2, arg3);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }

    public static async Task<TRes?> RaiseFirstOrDefaultAsync<TRes>(this Func<Task<TRes>> onEvent)
    {
        if (onEvent != null)
        {
            foreach (Func<Task<TRes>> action in onEvent.GetInvocationList())
            {
                try
                {
                    var res = await action();

                    if (res != null)
                        return res;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        return default;
    }

    public static async Task<TRes?> RaiseFirstOrDefaultAsync<T1, TRes>(this Func<T1, Task<TRes>> onEvent, T1 arg1)
    {
        if (onEvent != null)
        {
            foreach (Func<T1, Task<TRes>> action in onEvent.GetInvocationList())
            {
                try
                {
                    var res = await action(arg1);

                    if (res != null)
                        return res;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        return default;
    }
}
