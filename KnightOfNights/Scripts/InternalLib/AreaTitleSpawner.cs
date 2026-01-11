using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class AreaTitleSpawner : MonoBehaviour, IPersistentBehaviour<AreaTitleSpawner, AreaTitleSpawnerManager>
{
    [ShimField] public string AreaKey = "";
    [ShimField] public string PDBool = "";

    public void AwakeWithManager(AreaTitleSpawnerManager initManager)
    {
        var visited = PDBool == "" || PlayerData.instance.GetBool(PDBool);
        if (initManager.Trigger != null && !visited) initManager.Trigger.OnDetected(new Once(() => AreaTitleUtil.Spawn(AreaKey, PDBool)));
        else AreaTitleUtil.Spawn(AreaKey, PDBool);
    }

    public void SceneChanged(AreaTitleSpawnerManager newManager) { }

    public void Stop() => Destroy(gameObject);
}

[Shim]
internal class AreaTitleSpawnerManager : PersistentBehaviourManager<AreaTitleSpawner, AreaTitleSpawnerManager>
{
    [ShimField] public HeroDetectorProxy? Trigger;

    public override AreaTitleSpawnerManager Self() => this;
}
