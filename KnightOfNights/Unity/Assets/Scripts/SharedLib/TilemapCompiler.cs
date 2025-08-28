using KnightOfNights.Scripts.Framework;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

namespace KnightOfNights.Scripts.SharedLib  // TODO: SharedLib
{
    internal class TilemapGrid : Grid
    {
        private readonly Tilemap tilemap;

        public TilemapGrid(Tilemap tilemap) => this.tilemap = tilemap;

        public bool Filled(int x, int y) => tilemap.GetTile(new Vector3Int(x, y, 0)) != null;

        public int Height() => tilemap.size.y;

        public int Width() => tilemap.size.x;

        public int ComputeHash()
        {
            int hash = 0;
            Hash.Update(ref hash, Width());
            Hash.Update(ref hash, Height());
            for (int x = 0; x < Width(); x++)
                for (int y = 0; y < Height(); y++)
                    Hash.Update(ref hash, Filled(x, y));

            return hash;
        }
    }

    static class RectExtensions
    {
        public static List<Vector2> Points(this Rect r) => new List<Vector2>() { new Vector2(-r.W / 2f, -r.H / 2f), new Vector2(r.W / 2f, -r.H / 2f), new Vector2(r.W / 2f, r.H / 2f), new Vector2(-r.W / 2f, r.H / 2f), new Vector2(-r.W / 2f, -r.H / 2f) };

        public static Vector3 Center(this Rect r) => new Vector3(r.X + r.W / 2.0f, r.Y + r.H / 2.0f);
    }

    [RequireComponent(typeof(Tilemap))]
    public class TilemapCompiler : SceneDataOptimizer
    {
        public int TileHash;

        public override bool Optimize() => CompileTilemap();

#if UNITY_EDITOR
        [ContextMenu("Print Size")]
#endif
        void PrintSize()
        {
            var tilemap = GetComponent<Tilemap>();
            UnityEngine.Debug.Log($"Tilemap size: {tilemap.size.x}, {tilemap.size.y}");
        }

#if UNITY_EDITOR
        [ContextMenu("Compile Tilemap")]
#endif
        bool CompileTilemap()
        {
            bool changed = false;

            var renderer = GetComponent<TilemapRenderer>();
            if (renderer.sortingLayerName != "Tiles")
            {
                changed = true;
                renderer.sortingLayerName = "Tiles";
            }

            var tilemap = gameObject.GetComponent<Tilemap>();
            tilemap.CompressBounds();
            tilemap.color = new Color(1, 1, 1);  // Always set to white

            var grid = new TilemapGrid(tilemap);
            var newHash = grid.ComputeHash();
            if (newHash != TileHash)
            {
                TileHash = newHash;
                changed = true;
            }

            if (!changed) return false;

            if (gameObject.GetComponent<TilemapPatcher>() == null) gameObject.AddComponent<TilemapPatcher>();

            var compiled = gameObject.ResetCompiled();
            GameObject colliders = new GameObject();
            colliders.name = "Colliders";
            colliders.transform.SetParent(compiled.transform);

            int i = 0;
            foreach (var rect in TilemapCovering.ComputeCovering(grid))
            {
                GameObject go = new GameObject();
                go.name = $"Collider {++i}";
                go.layer = 8;  // Terrain
                go.transform.SetParent(colliders.transform);

                var ec2d = go.AddComponent<EdgeCollider2D>();
                ec2d.isTrigger = false;
                ec2d.SetPoints(rect.Points());
                go.transform.position = rect.Center();
            }

            return true;
        }

#if UNITY_EDITOR
        [ContextMenu("Monochromatize")]
#endif
        void Monochromatize()
        {
            var tilemap = gameObject.GetComponent<Tilemap>();
            var w = tilemap.size.x;
            var h = tilemap.size.y;
            TileBase firstTile = null;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    var tile = tilemap.GetTile(new Vector3Int(x, y, 0));
                    if (tile != null)
                    {
                        firstTile = (firstTile ?? tile);
                        tilemap.SetTile(new Vector3Int(x, y, 0), firstTile);
                    }
                }

            UnityEditorShims.MarkActiveSceneDirty();
        }
    }
}
