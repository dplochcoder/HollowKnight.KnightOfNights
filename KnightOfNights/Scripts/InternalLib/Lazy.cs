using System;

namespace KnightOfNights.Scripts.InternalLib;

internal class Lazy<T>
{
    private readonly Func<T> supplier;
    private T? value;

    public Lazy(Func<T> supplier) { this.supplier = supplier; }

    public T Get()
    {
        value ??= supplier();
        return value;
    }
}
