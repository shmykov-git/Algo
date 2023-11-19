using System;

namespace Model.Hashes;

public struct Hashed<T> : IEquatable<T>
{
    private readonly Func<T, int> getHashCodeFn;
    private readonly Func<T, T, bool> equalsFn;

    public T Item { get; init; }

    public Hashed(T item, Func<T, int> getHashCodeFn, Func<T, T, bool> equalsFn)
    {
        this.Item = item;
        this.getHashCodeFn = getHashCodeFn;
        this.equalsFn = equalsFn;
    }

    public override int GetHashCode() => getHashCodeFn(Item);

    public override bool Equals(object other) => equalsFn(Item, ((Hashed<T>)other).Item);

    public bool Equals(T other) => equalsFn(Item, other);
}
