using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightOfNights.Scripts.SharedLib
{
    internal class Dummy : MonoBehaviour
    {
        public new void StartCoroutine(IEnumerator coroutine) => base.StartCoroutine(coroutine);
    }

    public static class GameObjectExtensions
    {
        public static void SetVelocityX(this Rigidbody2D self, float x)
        {
            var v = self.velocity;
            v.x = x;
            self.velocity = v;
        }

        public static void SetVelocityY(this Rigidbody2D self, float y)
        {
            var v = self.velocity;
            v.y = y;
            self.velocity = v;
        }

        public static GameObject Coalesce(this GameObject self) => self == null ? null : self;

        public static void SetParent(this GameObject self, GameObject parent) => self.transform.SetParent(parent.transform);

        public static void Unparent(this GameObject self) => self.transform.parent = null;

        public static GameObject Parent(this GameObject self) => self.transform.parent?.gameObject;

        public static T FindParent<T>(this GameObject self) where T : Component
        {
            var obj = self.transform.parent?.gameObject;
            while (obj != null)
            {
                var component = obj.GetComponent<T>();
                if (component != null) return component;

                obj = obj.transform.parent?.gameObject;
            }
            return null;
        }

        public static void OffsetParent(this GameObject self, Vector3 offset)
        {
            foreach (var child in self.Children()) child.transform.position -= offset;
            self.transform.position += offset;
        }

        public static bool Contains(this BoxCollider2D self, Vector2 vec, float xBuffer = 0, float yBuffer = 0)
        {
            var b = self.bounds;
            var x1 = b.min.x - xBuffer;
            var y1 = b.min.y - yBuffer;
            var x2 = b.max.x + xBuffer;
            var y2 = b.max.y + yBuffer;

            return vec.x >= x1 && vec.x <= x2 && vec.y >= y1 && vec.y <= y2;
        }

        private const float CIRCLE_RESOLUTION = 0.15f;

        public static IEnumerable<Vector2> EnumeratePoints(this Collider2D self)
        {
            Vector2 TP(Vector2 p) => self.gameObject.transform.TransformPoint(p + self.offset);
            Vector2 TPCoord(float x, float y) => TP(new Vector2(x, y));

            if (self is BoxCollider2D box)
            {
                var size = box.size;
                yield return TPCoord(size.x / 2, size.y / 2);
                yield return TPCoord(-size.x / 2, size.y / 2);
                yield return TPCoord(-size.x / 2, -size.y / 2);
                yield return TPCoord(size.x / 2, -size.y / 2);
            }
            else if (self is EdgeCollider2D edge) foreach (var p in edge.points) yield return TP(p);
            else if (self is PolygonCollider2D polygon) foreach (var p in polygon.points) yield return TP(p);
            else if (self is CircleCollider2D circle)
            {
                var scale = self.gameObject.transform.localScale;
                int resolution = Mathf.CeilToInt(circle.radius * 2 * Mathf.PI * (Mathf.Abs(scale.x) + Mathf.Abs(scale.y)) / CIRCLE_RESOLUTION);
                for (int i = 0; i < resolution; i++)
                {
                    var angle = i * 360f / resolution;
                    yield return TP(angle.AsAngleToVec() * circle.radius);
                }
            }
            else throw new ArgumentException($"Unknown collider type: {self.GetType()}");
        }

        public static T GetOrAddComponent<T>(this GameObject self) where T : Component => self.GetComponent<T>() ?? self.AddComponent<T>();
        public static T GetOrAddComponentSharedLib<T>(this GameObject self) where T : Component => self.GetOrAddComponent<T>();

        public static IEnumerable<T> GetComponentsInChildren<T>(this Scene self, bool inactive = false) where T : Component
        {
            foreach (var obj in self.GetRootGameObjects()) foreach (var t in obj.GetComponentsInChildren<T>(inactive)) yield return t;
        }

        public static IEnumerable<T> FindInterfacesRecursive<T>(this GameObject self, bool inactive = false)
        {
            foreach (var component in self.GetComponentsInChildren<Component>(inactive)) if (component is T t) yield return t;
        }

        public static IEnumerable<T> FindInterfacesRecursive<T>(this Scene self, bool inactive = false)
        {
            foreach (var obj in self.GetRootGameObjects()) foreach (var component in obj.FindInterfacesRecursive<T>(inactive)) yield return component;
        }

        public static IEnumerable<T> FindInterfacesInScene<T>(bool inactive = false) => UnityEngine.SceneManagement.SceneManager.GetActiveScene().FindInterfacesRecursive<T>(inactive);

        public static GameObject SharedFindChild(this GameObject self, string name)
        {
            foreach (Transform child in self.transform) if (child.gameObject.name == name) return child.gameObject;
            return null;
        }

        public static IEnumerable<GameObject> Children(this GameObject self)
        {
            foreach (Transform child in self.transform) yield return child.gameObject;
        }

        public static IEnumerable<GameObject> RecursiveChildren(this GameObject self, Func<GameObject, bool> filter = null)
        {
            if (filter != null && !filter(self)) yield break;

            var queue = new Queue<GameObject>();
            queue.Enqueue(self);
            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();
                yield return obj;

                foreach (var child in obj.Children()) if (filter == null || filter(child)) queue.Enqueue(child);
            }
        }

        public static IEnumerable<GameObject> AllGameObjects(this Scene scene, Func<GameObject, bool> filter = null)
        {
            foreach (var root in scene.GetRootGameObjects()) foreach (var obj in root.RecursiveChildren(filter)) yield return obj;
        }

        public static void DestroyChildrenImmediate(this GameObject self, Func<GameObject, bool> filter = null)
        {
            var children = new List<GameObject>(self.Children());
            foreach (var child in children) if (filter == null || filter(child)) UnityEngine.Object.DestroyImmediate(child, true);
        }

        public static void DoAfter(this GameObject self, Action action, float delay)
        {
            IEnumerator Routine()
            {
                yield return new WaitForSeconds(delay);
                action();
            }
            self.GetOrAddComponent<Dummy>().StartCoroutine(Routine());
        }

        public static void DestroyAfter(this GameObject self, float delay) => self.DoAfter(() => UnityEngine.Object.Destroy(self), delay);

        public static GameObject ResetCompiled(this GameObject self, string name = "Compiled")
        {
            var compiled = self.SharedFindChild(name);
            if (compiled != null) UnityEngine.Object.DestroyImmediate(compiled);

            compiled = new GameObject(name);
            compiled.SetParent(self);
            compiled.transform.position = Vector3.zero;
            return compiled;
        }
    }
}
