using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

internal class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
{
    internal static T Get()
    {
        var name = typeof(T).Name;
        var obj = GameObject.Find(name);
        if (obj != null) return obj.GetComponent<T>();

        obj = new(name);
        return obj.AddComponent<T>();
    }
}
