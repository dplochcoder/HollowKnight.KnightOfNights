using System;
using System.Collections.Generic;

namespace KnightOfNights.Scripts.InternalLib;

internal class CompactDict<T>
{
    private T[]? array;

    public bool TryGetValue(int index, out T value)
    {
        if (array == null || index >= array.Length)
        {
            value = default;
            return false;
        }

        value = array[index];
        return true;
    }

    public void Set(int index, T value)
    {
        if (array == null) array = new T[index + 1];
        else if (index >= array.Length)
        {
            var prev = array;
            array = new T[index + 1];
            Array.Copy(prev, array, prev.Length);
        }

        array[index] = value;
    }

    public T this[int i]
    {
        get => TryGetValue(i, out var value) ? value : throw new KeyNotFoundException($"{i}");
        set => Set(i, value);
    }
}
