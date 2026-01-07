using System.Collections.Generic;

namespace KnightOfNights.Scripts.SharedLib
{
    public class WeightedDistribution<T>
    {
        private readonly List<T> elements = new List<T>();
        private readonly List<float> thresholds = new List<float>();
        public readonly float Sum;

        public WeightedDistribution(IEnumerable<(T, float)> elements)
        {
            float sum = 0;
            thresholds.Add(0);

            foreach (var (i, w) in elements)
            {
                if (w <= 0) continue;

                this.elements.Add(i);
                sum += w;
                thresholds.Add(sum);
            }
            Sum = sum;
        }

        public IList<T> Elements => elements;

        public T Choose(System.Random random)
        {
            if (elements.Count == 0) return default;

            var w = (float)(random.NextDouble() * Sum);

            int lo = 0;
            int hi = thresholds.Count;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) / 2;
                var v = thresholds[mid];
                if (v < w) lo = mid;
                else if (v > w) hi = mid;
            }

            return elements[lo];
        }
    }
}
