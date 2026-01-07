using UnityEngine;

namespace KnightOfNights.Scripts.SharedLib
{
    public static class UnityEditorShims
    {
#if UNITY_EDITOR
        private static void ReplaceWithPrefab(GameObject src, string prefabPath)
        {
            var pos = src.transform.position;
            var rot = src.transform.localRotation;
            var scale = src.transform.localScale;
            var name = src.name;
            var parent = src.transform.parent;

            var newObj = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(Resources.Load(prefabPath));
            if (newObj == null) return;

            Object.DestroyImmediate(src);
            newObj.name = name;
            if (parent != null) newObj.transform.SetParent(parent);
            newObj.transform.position = pos;
            newObj.transform.localRotation = rot;
            newObj.transform.localScale = scale;

            MarkDirty(newObj);
        }

        // [UnityEditor.MenuItem("GameObject/2D Object/ReplaceBeltSprite")]
        // public static void ReplaceBeltSprite(UnityEditor.MenuCommand command) => ReplaceWithPrefab(command.context as GameObject, "GameObjects/CrystalCore/Conveyor/BeltSprite");

        public static GameObject InstantiateMaybePrefab(this GameObject src, Transform parent)
        {
            if (UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(src))
            {
                var obj = UnityEditor.PrefabUtility.InstantiatePrefab(UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(src), parent);
                UnityEditor.PrefabUtility.SetPropertyModifications(obj, UnityEditor.PrefabUtility.GetPropertyModifications(src));
                return (GameObject)obj;
            }
            else return Object.Instantiate(src, parent);
        }

        public static void DeleteAll<T>() where T : Component
        {
            foreach (var t in Object.FindObjectsOfType<T>(true)) Object.DestroyImmediate(t);
        }

        private static GameObject swapper;

        private static void SwapImpl(GameObject obj)
        {
            if (swapper == null)
            {
                swapper = obj;
                return;
            }

            var tmp = swapper.name;
            swapper.name = obj.name;
            obj.name = tmp;
            swapper = null;
        }

        [UnityEditor.MenuItem("GameObject/2D Object/Swap Names")]
        public static void Swap(UnityEditor.MenuCommand command) => SwapImpl(command.context as GameObject);

        [UnityEditor.MenuItem("CONTEXT/BoxCollider2D/Snap1")]
        public static void SnapBox(UnityEditor.MenuCommand command) => MathExt.Snap(command.context as BoxCollider2D, 1f);

        [UnityEditor.MenuItem("CONTEXT/PolygonCollider2D/Snap1")]
        public static void SnapPolygon(UnityEditor.MenuCommand command) => MathExt.Snap(command.context as PolygonCollider2D, 1f);

        [UnityEditor.MenuItem("CONTEXT/BoxCollider2D/Snap0.5")]
        public static void SnapBoxHalf(UnityEditor.MenuCommand command) => MathExt.Snap(command.context as BoxCollider2D, 0.5f);

        [UnityEditor.MenuItem("CONTEXT/PolygonCollider2D/Snap0.5")]
        public static void SnapPolygonHalf(UnityEditor.MenuCommand command) => MathExt.Snap(command.context as PolygonCollider2D, 0.5f);

        [UnityEditor.MenuItem("CONTEXT/Transform/Reset Zero")]
        public static void ResetZero(UnityEditor.MenuCommand command) => MathExt.ResetZero(command.context as Transform);

        [UnityEditor.MenuItem("CONTEXT/Transform/Reset Avg")]
        public static void ResetAverage(UnityEditor.MenuCommand command) => MathExt.ResetAverage(command.context as Transform);

        [UnityEditor.MenuItem("CONTEXT/Transform/Flip X")]
        public static void FlipX(UnityEditor.MenuCommand command)
        {
            var t = command.context as Transform;
            var scale = t.localScale;
            scale.x *= -1;
            t.localScale = scale;
        }

        [UnityEditor.MenuItem("CONTEXT/Transform/Flip Y")]
        public static void FlipY(UnityEditor.MenuCommand command)
        {
            var t = command.context as Transform;
            var scale = t.localScale;
            scale.y *= -1;
            t.localScale = scale;
        }

        [UnityEditor.MenuItem("CONTEXT/HealthManager/Split Total Geo")]
        public static void SplitGeo(UnityEditor.MenuCommand command)
        {
            var health = command.context as Proxy.HealthManagerProxy;
            if (health == null) return;

            var (s, m, l) = MathExt.SplitGeo(health.TotalGeo);
            health.smallGeoDrops = s;
            health.mediumGeoDrops = m;
            health.largeGeoDrops = l;
            MarkDirty(health);
        }
#endif

        public static bool UpdateLighting()
        {
#if UNITY_EDITOR
            bool changed = false;
            if (RenderSettings.skybox != null)
            {
                RenderSettings.skybox = null;
                changed = true;
            }

            if (RenderSettings.ambientLight != Color.white)
            {
                RenderSettings.ambientLight = Color.white;
                changed = true;
            }

            return changed;
#else
            return false;
#endif
        }

        public static string GetAssetPath(Object obj)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
            return "";
#endif
        }

        public static T LoadAssetAtPath<T>(string path) where T : class
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
#else
            return null;
#endif
        }

        public static void MarkActiveSceneDirty()
        {
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
        }

        public static void MarkDirty(Object obj)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(obj);
#endif
        }

        public static bool RemoveMissingScripts(GameObject obj)
        {
#if UNITY_EDITOR
            return UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj) > 0;
#else
            return false;
#endif
        }
    }
}