using KnightOfNights.Scripts.SharedLib;
using SFCore.MonoBehaviours;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Lib
{
#if UNITY_EDITOR
    public class OptimizerFinder
    {
        static OptimizerFinder()
        {
            SceneDataOptimizer.RegisterType<Deactivator>(o => o.Deactivate());
        }

        private enum FixResult
        {
            UNCHANGED,
            CHANGED,
            DELETED,
        }

        private static FixResult ChangedResult(bool changed) => changed ? FixResult.CHANGED : FixResult.UNCHANGED;

        private static bool UpdateFloat(ref float src, float dest)
        {
            if (Mathf.Abs(src - dest) < 0.001f) return false;

            src = dest;
            return true;
        }

        private static bool OptimizeObject(Object o, bool optimized)
        {
            if (optimized) UnityEditorShims.MarkDirty(o);
            return optimized;
        }

        private static bool FixAll<T>(System.Func<T, FixResult> fixer) where T : Component
        {
            bool changed = false;
            foreach (var t in Object.FindObjectsOfType<T>(true))
            {
                var result = fixer(t);
                changed |= result != FixResult.UNCHANGED;
                if (result == FixResult.CHANGED) UnityEditorShims.MarkDirty(t);
            }
            return changed;
        }

        private static bool FixSDOs()
        {
            bool changed = false;

            var map = new Dictionary<int, HashSet<SceneDataOptimizer>>();
            foreach (var sdo in Object.FindObjectsOfType<SceneDataOptimizer>(true)) map.GetOrAddNew(sdo.Priority).Add(sdo);

            var keys = new List<int>(map.Keys);
            keys.Sort();
            foreach (var key in keys)
            {
                foreach (var sdo in map[key])
                {
                    var result = sdo.Optimize();
                    changed |= sdo.Optimize();
                    if (result) UnityEditorShims.MarkDirty(sdo);
                }
            }

            foreach (var component in Object.FindObjectsOfType<Component>(true))
            {
                var result = SceneDataOptimizer.OptimizeCustom(component);
                changed |= result;
                if (result) UnityEditorShims.MarkDirty(component);
            }
            return changed;
        }

        private static bool FixGraph<T>(System.Func<T, List<T>> deps, System.Func<T, FixResult> fixer) where T : Component
        {
            bool changed = false;

            var graph = new Dictionary<T, HashSet<T>>();
            var invGraph = new Dictionary<T, HashSet<T>>();
            var queue = new Queue<T>();
            var visited = new HashSet<T>();
            foreach (var t in Object.FindObjectsOfType<T>(true))
            {
                var ds = deps(t);
                if (ds.Count == 0)
                {
                    queue.Enqueue(t);
                    visited.Add(t);
                    continue;
                }

                graph.Add(t, new HashSet<T>(ds));
                foreach (var d in ds) invGraph.GetOrAddNew(d).Add(t);
            }

            while (queue.Count > 0)
            {
                var t = queue.Dequeue();
                foreach (var i in invGraph.GetOrAddNew(t))
                {
                    var ds = graph.GetOrAddNew(i);
                    if (ds.Remove(t) && ds.Count == 0)
                    {
                        queue.Enqueue(i);
                        graph.Remove(i);
                    }
                }

                var result = fixer(t);
                if (result == FixResult.CHANGED) UnityEditorShims.MarkDirty(t);
                changed |= result != FixResult.UNCHANGED;
            }

            foreach (var entry in graph)
                throw new System.ArgumentException($"{entry.Key.gameObject.name} is part of a cycle");

            return changed;
        }

        public static List<string> FixScene()
        {
            var updates = new List<string>();
            void Update(string name, bool fnResult)
            {
                if (fnResult) updates.Add(name);
            };

            Update("AddRequiredObjects()", AddRequiredObjects());
            Update("Lighting()", UpdateLighting());
            Update("RemoveObsoleteObjects()", RemoveObsoleteObjects());
            Update("FixScenery()", FixScenery());
            Update("FixTilemapScript()", FixTilemapScript());
            Update("FixAll<CameraLockAreaProxy>(...)", FixAll<CameraLockAreaProxy>(FixCLAP));
            Update("FixAll<HeroDetectorProxy>(...)", FixAll<HeroDetectorProxy>(FixHDP));
            Update("FixAll<HazardRespawnTrigger>(...)", FixAll<HazardRespawnTrigger>(FixHRT));
            Update("FixSDOs()", FixSDOs());
            Update("FixAll<TransitionPoint>(...)", FixAll<TransitionPoint>(FixTP));

            return updates;
        }

        private static bool AddRequiredObjects()
        {
            var objects = new string[] { "_Transition Gates", "Darkness", "CameraLocks", "HRTs", "Secrets", "ShadeMarkers" };

            bool changed = false;
            foreach (var name in objects)
            {
                var go = GameObject.Find(name);
                if (go == null)
                {
                    go = new GameObject(name);
                    UnityEditorShims.MarkDirty(go);
                    changed = true;
                }
            }
            return changed;
        }

        private static bool UpdateLighting() => UnityEditorShims.UpdateLighting();

        private static bool RemoveObsoleteObjects()
        {
            var objects = new string[] { };

            bool changed = false;
            foreach (var name in objects)
            {
                var go = GameObject.Find(name);
                if (go != null)
                {
                    UnityEditorShims.MarkDirty(go);
                    Object.DestroyImmediate(go);
                    changed = true;
                }
            }
            return changed;
        }

        private static readonly Dictionary<string, int> SortingLayers = new Dictionary<string, int>()
        {
            {"Default", 0},
            {"Far BG 2", 1},  // useless
            {"Far BG 1", 2},  // useless
            {"Mid BG", 3},    // useless
            {"Immediate BG", 4},  // useless
            {"Actors", 5},  // useless
            {"Player", 6},  // useless
            {"Tiles", 7},
            {"MID Dressing", 8},
            {"Immediate FG", 9}, // z >= 0
            {"Far FG", 10}, // z < 0
            {"Vignette", 11},
            {"Over", 12},
            {"HUD", 13},
        };

        private static bool IsUseless(int depthIndex) => depthIndex >= 1 && depthIndex <= 6;

        private static bool FixScenery()
        {
            var go = GameObject.Find("_Scenery");
            if (go == null)
            {
                go = new GameObject("_Scenery");
                go.AddComponent<SpritePatcher>();
                return true;
            }

            bool changed = false;
            foreach (var spriteRenderer in go.GetComponentsInChildren<SpriteRenderer>(true))
            {
                var quat = spriteRenderer.gameObject.transform.localRotation;
                var ea = quat.eulerAngles;
                if (ea.x != 0 || ea.y != 0)
                {
                    ea.x = 0;
                    ea.y = 0;
                    spriteRenderer.gameObject.transform.localRotation = Quaternion.Euler(ea);
                    changed = true;
                }

                int depthIndex = SortingLayers[spriteRenderer.sortingLayerName];
                float z = spriteRenderer.gameObject.transform.position.z;
                if (z < 0)
                {
                    if (depthIndex < SortingLayers["Far FG"])
                    {
                        spriteRenderer.sortingLayerName = "Far FG";
                        changed = true;
                    }
                }
                else if (z == 0)
                {
                    if (depthIndex > SortingLayers["Immediate FG"])
                    {
                        spriteRenderer.sortingLayerName = "Immediate FG";
                        changed = true;
                    }
                    if (IsUseless(depthIndex))
                    {
                        spriteRenderer.sortingLayerName = "Default";
                        changed = true;
                    }
                }
                else // z > 0
                {
                    if (spriteRenderer.sortingLayerName != "Default")
                    {
                        spriteRenderer.sortingLayerName = "Default";
                        changed = true;
                    }
                    if (IsUseless(depthIndex))
                    {
                        spriteRenderer.sortingLayerName = "Default";
                        changed = true;
                    }
                }
            }

            // TODO: Verify no inversions.

            if (go.GetComponent<SpritePatcher>() == null)
            {
                go.AddComponent<SpritePatcher>();
                changed = true;
            }

            return changed;
        }

        private static bool FixTilemapScript()
        {
            bool changed = false;

            var grid = GameObject.Find("TilemapGrid");
            if (grid == null)
            {
                grid = GameObject.Find("Tilemap2");
                if (grid != null)
                {
                    grid.name = "TilemapGrid";
                    changed = true;
                    UnityEditorShims.MarkDirty(grid);
                }
            }
            var tilemap = grid?.SharedFindChild("Tilemap");
            if (tilemap == null) return false;

            if (UnityEditorShims.RemoveMissingScripts(tilemap))
            {
                changed = true;
                UnityEditorShims.MarkDirty(tilemap);
            }

            if (tilemap.GetComponent<TilemapCompiler>() == null)
            {
                tilemap.AddComponent<TilemapCompiler>();
                UnityEditorShims.MarkDirty(tilemap);
                changed = true;
            }

            return changed;
        }

        private static FixResult FixCLAP(CameraLockAreaProxy clap)
        {
            bool changed = false;
            if (MathExt.NeedsSnap(clap.gameObject.transform.position, 0.5f))
            {
                clap.gameObject.transform.position = MathExt.Snap(clap.gameObject.transform.position, 0.5f);
                changed = true;
            }

            foreach (Transform child in clap.gameObject.transform.parent.gameObject.transform)
            {
                var b2d = child.gameObject.GetComponent<BoxCollider2D>();
                if (MathExt.Snap(b2d, 1f))
                {
                    changed = true;
                    UnityEditorShims.MarkDirty(b2d.gameObject);
                }
            }

            return ChangedResult(changed);
        }

        private static FixResult FixHDP(HeroDetectorProxy hdp)
        {
            bool changed = false;

            if (hdp.gameObject.layer != 13)
            {
                changed = true;
                hdp.gameObject.layer = 13;
            }

            foreach (var collider in hdp.gameObject.GetComponents<Collider2D>())
            {
                if (!collider.isTrigger)
                {
                    collider.isTrigger = true;
                    changed = true;
                }
            }

            return ChangedResult(changed);
        }

        private static FixResult FixHRT(HazardRespawnTrigger hrt)
        {
            bool changed = false;
            var bc = hrt.gameObject.GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                changed |= !bc.isTrigger;
                bc.isTrigger = true;
            }

            if (hrt.respawnMarker == null)
            {
                bool isFixed = false;
                foreach (Transform child in hrt.gameObject.transform)
                {
                    var hrm = child.gameObject.GetComponent<HazardRespawnMarker>();
                    if (hrm != null)
                    {
                        hrt.respawnMarker = hrm;
                        isFixed = true;
                        changed = true;
                        break;
                    }
                }

                if (!isFixed) Debug.LogError($"{hrt.name} is missing its HazardRespawnMarker");
            }

            return ChangedResult(changed);
        }

        private static FixResult FixTP(TransitionPoint tp)
        {
            bool changed = false;
            var bc = tp.gameObject.GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                changed |= !bc.isTrigger;
                bc.isTrigger = true;
            }

            if (tp.respawnMarker == null)
            {
                bool isFixed = false;
                foreach (Transform child in tp.gameObject.transform)
                {
                    var hrm = child.gameObject.GetComponent<HazardRespawnMarker>();
                    if (hrm != null)
                    {
                        tp.respawnMarker = hrm;
                        isFixed = true;
                        changed = true;
                        break;
                    }
                }

                if (!isFixed) Debug.LogError($"{tp.name} is missing its HazardRespawnMarker");
            }

            return ChangedResult(changed);
        }
    }
#endif
}