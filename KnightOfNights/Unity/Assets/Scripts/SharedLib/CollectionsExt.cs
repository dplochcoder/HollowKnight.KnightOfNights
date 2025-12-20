using System;
using System.Collections.Generic;

namespace KnightOfNights.Scripts.SharedLib
{
    internal class EmptyCollection<T>
    {
        private static readonly List<T> Instance = new List<T>();

        public static IReadOnlyList<T> Get() => Instance;
    }

    public static class CollectionsExt
    {
        public static IReadOnlyList<T> Empty<T>() => EmptyCollection<T>.Get();

        public static void UpdateAll<T>(this List<T> self, Func<T, T> updater)
        {
            for (int i = 0; i < self.Count; i++) self[i] = updater(self[i]);
        }

        public static void Rotate<T>(this List<T> self, int offset)
        {
            var orig = new List<T>(self);
            for (int i = 0; i < self.Count; i++) self[i] = orig[(i + self.Count - offset) % self.Count];
        }

        public static void RemoveWhere<T>(this List<T> self, Func<T, bool> predicate)
        {
            List<T> newList = new List<T>();
            for (int i = 0; i < self.Count; i++) if (!predicate(self[i])) newList.Add(self[i]);
            if (newList.Count == self.Count) return;

            self.Clear();
            self.AddRange(newList);
        }

        public static bool RemoveWhere<K, V>(this IDictionary<K, V> self, Func<K, V, bool> predicate)
        {
            List<K> keys = new List<K>();
            foreach (var e in self) if (predicate(e.Key, e.Value)) keys.Add(e.Key);
            foreach (var k in keys) self.Remove(k);
            return keys.Count > 0;
        }

        // A more efficient List.Remove that doesn't preserve order.
        public static T RemoveUnordered<T>(this List<T> self, int index)
        {
            T ret = self[index];
            int last = self.Count - 1;
            if (index != last) self[index] = self[last];
            self.RemoveAt(last);
            return ret;
        }

        // Aggregate version of RemoveUnordered.
        public static void RemoveUnorderedWhere<T>(this List<T> self, Func<T, bool> predicate)
        {
            int i = 0;
            while (i < self.Count)
            {
                if (predicate(self[i])) self.RemoveUnordered(i);
                else i++;
            }
        }

        public static void Dedup<T>(this List<T> self)
        {
            var unique = new List<T>();
            var set = new HashSet<T>();
            foreach (var item in self) if (set.Add(item)) unique.Add(item);

            self.Clear();
            self.AddRange(unique);
        }

        public static V GetOrAddNew<K, V>(this IDictionary<K, V> self, K key) where V : new()
        {
            if (self.TryGetValue(key, out var value)) return value;

            var newValue = new V();
            self.Add(key, newValue);
            return newValue;
        }

        public static void Swap<T>(this List<T> self, int a, int b)
        {
            (self[b], self[a]) = (self[a], self[b]);
        }

        public static List<T> ToList<T>(this IEnumerator<T> self)
        {
            var ret = new List<T>();
            while (self.MoveNext()) ret.Add(self.Current);
            return ret;
        }

        public static T Choose<T>(this T[] self) => self[UnityEngine.Random.Range(0, self.Length)];

        public static T Choose<T>(this List<T> self) => self[UnityEngine.Random.Range(0, self.Count)];

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
        private static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> self, bool circular)
        {
            T firstItem = default;
            T prevItem = default;
            bool first = true;
            int count = 0;
            foreach (var elem in self)
            {
                ++count;

                if (first)
                {
                    first = false;
                    firstItem = elem;
                }
                else yield return (prevItem, elem);

                prevItem = elem;
            }

            if (circular && count > 1) yield return (prevItem, firstItem);
        }
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        public static IEnumerable<(T, T)> Pairs<T>(this IEnumerable<T> self) => self.Pairs(false);

        public static IEnumerable<(T, T)> CircularPairs<T>(this IEnumerable<T> self) => self.Pairs(true);

        public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
        {
            foreach (var item in self) action(item);
        }

        public static void ForEachCopied<T>(this IEnumerable<T> self, Action<T> action)
        {
            var list = new List<T>();
            list.AddRange(self);
            list.ForEach(action);
        }

        public static void SortBy<T, C>(this List<T> self, Func<T, C> extractor) where C : IComparable => self.Sort((a, b) => extractor(a).CompareTo(extractor(b)));

        public static List<T> FilterSimilar<T>(this List<T> list, Func<T, T, bool> similar)
        {
            if (list.Count == 0) return list;

            var ret = new List<T>
            {
                list[0]
            };
            for (int i = 1; i < list.Count; i++) if (!similar(ret[ret.Count - 1], list[i])) ret.Add(list[i]);

            return ret;
        }
    }
}
