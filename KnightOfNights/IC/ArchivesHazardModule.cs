using GlobalEnums;
using ItemChanger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class ArchivesHazardModule : AbstractModule<ArchivesHazardModule>
{
    protected override ArchivesHazardModule Self() => this;

    public override void Initialize()
    {
        base.Initialize();
        Events.AddSceneChangeEdit(SceneNames.Fungus3_archive_02, AddHazard);
    }

    public override void Unload()
    {
        Events.RemoveSceneChangeEdit(SceneNames.Fungus3_archive_02, AddHazard);
        base.Unload();
    }

    private void AddHazard(Scene scene)
    {
        GameObject obj = new("HRT");
        obj.layer = (int)PhysLayers.HERO_DETECTOR;
        var trigger = obj.AddComponent<HazardRespawnTrigger>();
        trigger.transform.position = new(30, 63);
        var box = obj.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new(4, 4);

        GameObject obj2 = new("HRM");
        var marker = obj2.AddComponent<HazardRespawnMarker>();
        marker.transform.position = new(30, 63);
        marker.respawnFacingRight = true;
        trigger.respawnMarker = marker;
    }
}
