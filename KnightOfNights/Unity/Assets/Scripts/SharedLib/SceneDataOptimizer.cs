using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.SharedLib
{
    public abstract class SceneDataOptimizer : MonoBehaviour
    {
        private static readonly Dictionary<Type, Func<Component, bool>> customOptimizers = new Dictionary<Type, Func<Component, bool>>();

        public static void RegisterType<T>(Func<T, bool> func) where T : Component => customOptimizers.Add(typeof(T), obj => func(obj as T));

        public static bool OptimizeCustom(Component c) => customOptimizers.TryGetValue(c.GetType(), out var func) ? func(c) : false;

        public abstract bool Optimize();

        public virtual int Priority => 1;
    }
}
