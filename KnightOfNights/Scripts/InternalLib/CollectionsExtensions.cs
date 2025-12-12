using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class CollectionsExtensions
{
    public static T? MaybeMoveNext<T>(this IEnumerator<T> self) => self.MoveNext() ? self.Current : default;

    public static T AggregateOrDefault<T>(this IEnumerable<T> self, Func<T, T, T> aggregator) where T : struct
    {
        T value = default;
        foreach (var item in self) value = aggregator(value, item);
        return value;
    }

    public static Vector2 Sum(this IEnumerable<Vector2> self) => self.AggregateOrDefault((a, b) => a + b);

    public static Vector3 Sum(this IEnumerable<Vector3> self) => self.AggregateOrDefault((a, b) => a + b);
}
