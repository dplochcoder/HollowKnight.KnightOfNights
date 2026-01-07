using KnightOfNights.Scripts.SharedLib;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.Lib
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class CaveBuilder : SceneDataOptimizer
    {
        public int Seed;
        public int CompiledHash;

        public AssetBrushGroup BrushGroup;
        public float Density;
        public float Granularity;
        public float MaxGap;
        public float MinScale;
        public float MaxScale;

        private const int VERSION = 1;

        public override bool Optimize()
        {
            int hash = Seed;
            Hash.Update(ref hash, VERSION);
            Hash.Update(ref hash, Density);
            Hash.Update(ref hash, Granularity);
            Hash.Update(ref hash, MaxGap);
            Hash.Update(ref hash, BrushGroup.Instances.Count);
            Hash.Update(ref hash, MinScale);
            Hash.Update(ref hash, MaxScale);

            int nodeHash = 0;
            foreach (var node in gameObject.GetComponentsInChildren<CaveBuilderNode>())
            {
                int h = 0;
                Hash.Update(ref h, (Vector2)node.transform.position);
                Hash.Update(ref h, node.Depth);
                nodeHash += h;
            }
            Hash.Update(ref hash, nodeHash);

            foreach (var point in gameObject.GetComponent<PolygonCollider2D>().EnumeratePoints()) Hash.Update(ref hash, point);

            if (CompiledHash == hash) return false;

            var random = new System.Random();
            BuildCave(gameObject.ResetCompiled(), new System.Random());

            CompiledHash = hash;
            return true;
        }

        private void BuildCave(GameObject root, System.Random r)
        {
#if UNITY_EDITOR
            var nodes = gameObject.GetComponentsInChildren<CaveBuilderNode>();
            if (nodes.Length == 0) return;

            float ComputeDepth(Vector2 pos)
            {
                if (nodes.Length == 1) return nodes[0].Depth;

                var ordered = nodes.OrderBy(n =>
                {
                    Vector2 np = n.transform.position;
                    return (pos - np).sqrMagnitude;
                }).ToList();

                var p1 = ordered[0].transform.position;
                var p2 = ordered[1].transform.position;
                var t = MathExt.ILerp(pos, p1, p2);

                var d1 = ordered[0].Depth;
                var d2 = ordered[1].Depth;
                return d1 + t * (d2 - d1);
            }

            var collider = gameObject.GetComponent<PolygonCollider2D>();
            var polygon = CreateNTSPolygon(collider);

            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
            Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);
            foreach (var p in collider.EnumeratePoints())
            {
                min.x = System.Math.Min(min.x, Mathf.FloorToInt(p.x / Granularity));
                min.y = System.Math.Min(min.y, Mathf.FloorToInt(p.y / Granularity));
                max.x = System.Math.Max(max.x, Mathf.CeilToInt(p.x / Granularity));
                max.y = System.Math.Max(max.y, Mathf.CeilToInt(p.y / Granularity));
            }

            // Compute the density of each cell.
            List<(Vector2Int, float)> cellWeights = new List<(Vector2Int, float)>();
            for (int x = min.x; x < max.x; x++)
            {
                for (int y = min.y; y < max.y; y++)
                {
                    var p = new Vector2Int(x, y);
                    var rect = CreateNTSRect(p);
                    var intersection = polygon.Intersection(rect);
                    float avgDepth = ComputeDepth(new Vector2((x + 0.5f) * Granularity, (y + 0.5f) * Granularity));

                    cellWeights.Add((p, (float)intersection.Area * (1 + avgDepth / 38.1f)));
                }
            }
            var cellDistribution = new WeightedDistribution<Vector2Int>(cellWeights);
            int count = Mathf.CeilToInt(cellDistribution.Sum * Density);

            var placements = new Dictionary<Vector2Int, List<Vector2>>();
            void Place(Vector2Int cell)
            {
                var template = BrushGroup.Instances[r.Next(BrushGroup.Instances.Count)];
                var obj = UnityEditorShims.InstantiateMaybePrefab(template, root.transform);

                obj.transform.localScale *= r.Range(MinScale, MaxScale);
                if (r.CoinFlip())
                {
                    var s = obj.transform.localScale;
                    s.x *= -1;
                    obj.transform.localScale = s;
                }
                obj.transform.localRotation = Quaternion.Euler(0, 0, r.Range(360f));
                var x = r.Range(cell.x * Granularity, (cell.x + 1) * Granularity);
                var y = r.Range(cell.y * Granularity, (cell.y + 1) * Granularity);
                var pos = new Vector2(x, y);
                obj.transform.position = new Vector3(x, y, ComputeDepth(pos));

                if (placements.TryGetValue(cell, out var values)) values.Add(pos);
                else placements.Add(cell, new List<Vector2> { pos });
            }

            for (int i = 0; i < count; i++) Place(cellDistribution.Choose(r));

            var shuffled = new List<Vector2Int>(cellDistribution.Elements);
            r.Shuffle(shuffled);

            IEnumerable<Vector2Int> NeighborCells(Vector2Int center)
            {
                bool IsValid(Vector2Int cell) => cell.x >= min.x && cell.y >= min.y && cell.x <= max.x && cell.y <= max.y;

                int radius = 0;
                while (true)
                {
                    bool any = false;
                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        var cell = new Vector2Int(center.x + dx, center.y - radius);
                        if (IsValid(cell))
                        {
                            yield return cell;
                            any = true;
                        }
                        cell = new Vector2Int(center.x + dx, center.y + radius);
                        if (IsValid(cell))
                        {
                            yield return cell;
                            any = true;
                        }
                    }
                    for (int dy = 1 - radius; dy <= radius - 1; dy++)
                    {
                        var cell = new Vector2Int(center.x - radius, center.y + dy);
                        if (IsValid(cell))
                        {
                            yield return cell;
                            any = true;
                        }
                        cell = new Vector2Int(center.x + radius, center.y + dy);
                        if (IsValid(cell))
                        {
                            yield return cell;
                            any = true;
                        }
                    }

                    if (!any) yield break;
                    else ++radius;
                }
            }

            IEnumerable<Vector2> NeighborPlacements(Vector2Int center)
            {
                foreach (var cell in NeighborCells(center))
                {
                    if (placements.TryGetValue(cell, out var values))
                    {
                        foreach (var value in values) yield return value;
                    }
                }
            }

            bool HasGap(Vector2Int cell)
            {
                var pos = new Vector2((cell.x + 0.5f) * Granularity, (cell.y + 0.5f) * Granularity);
                foreach (var p in NeighborPlacements(cell))
                {
                    var d = (pos - p).magnitude;
                    if (d <= MaxGap / 2) return false;
                    if (d >= MaxGap) return true;
                }

                return true;
            }

            foreach (var cell in shuffled)
            {
                if (HasGap(cell)) Place(cell);
            }
#endif
        }

        private static Geometry CreateNTSPolygon(PolygonCollider2D collider)
        {
            List<Coordinate> coords = new List<Coordinate>();
            foreach (var p in collider.EnumeratePoints()) coords.Add(new Coordinate(p.x, p.y));
            coords.Add(coords[0]);

            return NtsGeometryServices.Instance.CreateGeometryFactory().CreatePolygon(coords.ToArray());
        }

        private Geometry CreateNTSRect(Vector2Int cell)
        {
            float x1 = cell.x * Granularity;
            float y1 = cell.y * Granularity;
            float x2 = (cell.x + 1) * Granularity;
            float y2 = (cell.y + 1) * Granularity;

            Coordinate[] coords = new Coordinate[]
            {
                new Coordinate(x1, y1),
                new Coordinate(x1, y2),
                new Coordinate(x2, y2),
                new Coordinate(x2, y1),
                new Coordinate(x1, y1),
            };
            return NtsGeometryServices.Instance.CreateGeometryFactory().CreatePolygon(coords);
        }

        static CaveBuilder()
        {
            NtsGeometryServices.Instance = new NtsGeometryServices(
                NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
                new PrecisionModel(PrecisionModels.FloatingSingle),
                srid: -1,
                GeometryOverlay.NG,
                new CoordinateEqualityComparer());
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.PostProcessScene]
        public static void DeleteMe() => UnityEditorShims.DeleteAll<CaveBuilder>();
#endif
    }
}
