using System.Collections.Generic;

namespace KnightOfNights.Scripts.InternalLib;

internal static class CollectionsExtensions
{
    public static T? MaybeMoveNext<T>(this IEnumerator<T> self) => self.MoveNext() ? self.Current : default;
}
