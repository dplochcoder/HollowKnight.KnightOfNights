using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightOfNights.Scripts.InternalLib;

interface IPersistentBehaviour<B, M> where B : MonoBehaviour, IPersistentBehaviour<B, M> where M : PersistentBehaviourManager<B, M>
{
    void AwakeWithManager(M initManager);

    void SceneChanged(M newManager);

    void Stop();
}

internal abstract class PersistentBehaviourManager<B, M> : MonoBehaviour where B : MonoBehaviour, IPersistentBehaviour<B, M> where M : PersistentBehaviourManager<B, M>
{
    [ShimField] public string Id = "";
    [ShimField] public GameObject? Prefab;

    private static readonly Dictionary<string, B> existing = [];

    public abstract M Self();

    public static bool TryGet(string id, out B instance)
    {
        if (existing.TryGetValue(id, out var obj) && obj.TryGetComponent<B>(out instance))
            return true;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        instance = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        return false;
    }

    private void Drop(B current)
    {
        current.Stop();
        current.DoAfter(() => Destroy(current.gameObject), 10f);
        if (!TryGet(Id, out var prev) || prev != current) return;

        existing.Remove(Id);
    }

    protected void Awake()
    {
        if (TryGet(Id, out var current))
        {
            current.SceneChanged(Self());
            return;
        }

        var obj = Instantiate(Prefab!);
        DontDestroyOnLoad(obj);

        current = obj.GetComponent<B>();
        existing[Id] = current;
        current.name = $"Persistent_{Id}";
        current.AwakeWithManager(Self());

        Util.Events.OnNextSceneChange += s => OnNextScene(current, s);
    }

    private void OnNextScene(B current, Scene scene)
    {
        if (scene.GetComponentsInChildren<M>(true).Any(m => m.Id == Id)) Util.Events.OnNextSceneChange += s => OnNextScene(current, s);
        else Drop(current);
    }
}