using System;

namespace Model
{
    public class DebugException<T> : Exception
    {
        public T Value { get; }

        public DebugException(T value)
        {
            Value = value;
        }
    }
}