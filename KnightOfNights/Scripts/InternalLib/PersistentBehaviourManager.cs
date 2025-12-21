using ItemChanger;
using KnightOfNights.Scripts.SharedLib;
using System;
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

    private static Action<Scene>? SceneChangedHandler
    {
        get => field;
        set
        {
            if (field == value) return;

            if (value != null) Events.OnSceneChange -= value;
            Events.OnSceneChange += value;
            field = value;
        }
    }
    private static GameObject? existing = null;

    public static B? Get() => existing?.GetComponent<B>();

    public abstract M Self();

    public static void Drop(B current)
    {
        current.Stop();
        current.DoAfter(() => Destroy(current.gameObject), 10f);
        if (existing?.gameObject != current.gameObject) return;

        existing = null;
        SceneChangedHandler = null;
    }

    protected void Awake()
    {
        var prevObj = Get();
        if (prevObj != null)
        {
            prevObj.SceneChanged(Self());
            Destroy(gameObject);
            return;
        }

        existing = Instantiate(Prefab!);
        existing!.name = $"Persistent_{Id}";
        Get()!.AwakeWithManager(Self());
        DontDestroyOnLoad(existing);

        SceneChangedHandler = scene =>
        {
            if (scene.GetComponentsInChildren<M>().Any(m => m.Id == Id)) return;
            Drop(existing.GetComponent<B>());
        };
    }
}