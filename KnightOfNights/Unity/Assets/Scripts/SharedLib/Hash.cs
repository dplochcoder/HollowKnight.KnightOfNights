using UnityEngine;

namespace KnightOfNights.Scripts.SharedLib
{
    internal static class Hash
    {
        public static void Update(ref int hash, byte next) => Update(ref hash, (int)next);

        public static void Update(ref int hash, byte[] next)
        {
            foreach (var b in next) Update(ref hash, b);
        }

        public static void Update(ref int hash, string next)
        {
            Update(ref hash, next.Length);
            foreach (var b in next.ToCharArray()) Update(ref hash, b);
        }

        public static void Update(ref int hash, int next) => hash = hash * 10007 + next + 11113;

        public static void Update(ref int hash, bool next) => Update(ref hash, next ? 1 : 0);

        public static void Update(ref int hash, float next) => Update(ref hash, System.BitConverter.GetBytes(next));

        public static void Update(ref int hash, Vector2 next)
        {
            Update(ref hash, next.x);
            Update(ref hash, next.y);
        }

        public static void Update(ref int hash, Vector3 next)
        {
            Update(ref hash, next.x);
            Update(ref hash, next.y);
            Update(ref hash, next.z);
        }
    }
}
