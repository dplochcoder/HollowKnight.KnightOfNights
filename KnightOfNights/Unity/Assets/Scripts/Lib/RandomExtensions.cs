using System.Collections.Generic;

namespace KnightOfNights.Scripts.Lib
{
    public static class RandomExtensions
    {
        public static float NextFloat(this System.Random self) => (float)self.NextDouble();

        public static float Range(this System.Random self, float max) => max * self.NextFloat();

        public static float Range(this System.Random self, float min, float max) => min + (max - min) * self.NextFloat();

        public static bool CoinFlip(this System.Random self) => self.Next(2) == 0;

        public static void Shuffle<T>(this System.Random self, List<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                var j = i + self.Next(list.Count - i);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
