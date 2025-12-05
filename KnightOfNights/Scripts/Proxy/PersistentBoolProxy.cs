using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
internal class PersistentBoolProxy : MonoBehaviour
{
    [ShimField] public string id = "";
    [ShimField] public string sceneName = "";
    [ShimField] public bool semiPersistent;

    private bool awoken;

    public void Awake()
    {
        if (awoken) return;
        awoken = true;

        static void Wrapper(On.PersistentBoolItem.orig_Awake orig, PersistentBoolItem self) { }
        On.PersistentBoolItem.Awake += Wrapper;
        var pbi = gameObject.AddComponent<PersistentBoolItem>();
        On.PersistentBoolItem.Awake -= Wrapper;

        pbi.semiPersistent = semiPersistent;

        pbi.persistentBoolData = new();
        var pbd = pbi.persistentBoolData;
        pbd.semiPersistent = semiPersistent;
        if (id.Length > 0)
        {
            pbd.id = id;
        }
        if (sceneName.Length > 0)
        {
            pbd.sceneName = sceneName;
        }

        pbi.PreSetup();
        Destroy(this);
    }

    public bool IsActivated() => gameObject.IsPBActivated();

    public void Activate() => gameObject.ActivatePB();

    public void SetPBActivation(bool activation) => gameObject.SetPBActivation(activation);
}

internal static class PersistentPoolExtensions
{
    public static bool HasPB(this GameObject obj) => obj.GetComponent<PersistentBoolItem>() != null || obj.GetComponent<PersistentBoolProxy>() != null;

    public static bool IsPBActivated(this GameObject obj)
    {
        obj.GetComponent<PersistentBoolProxy>()?.Awake();
        return obj.GetComponent<PersistentBoolItem>()?.persistentBoolData.activated ?? false;
    }

    public static bool SetPBActivation(this GameObject obj, bool activation)
    {
        obj.GetComponent<PersistentBoolProxy>()?.Awake();
        var pbi = obj.GetComponent<PersistentBoolItem>();
        if (pbi != null)
        {
            pbi.persistentBoolData.activated = activation;
            pbi.SaveState();
            return true;
        }
        return false;
    }

    public static bool ActivatePB(this GameObject obj) => obj.SetPBActivation(true);

    public static bool DeactivatePB(this GameObject obj) => obj.SetPBActivation(false);
}
