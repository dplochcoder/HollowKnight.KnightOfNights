using GlobalEnums;
using ItemChanger;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class ArchivesHazardModule : AbstractModule<ArchivesHazardModule>
{
    protected override ArchivesHazardModule Self() => this;

    protected override void InitializeInternal() => Events.AddSceneChangeEdit(SceneNames.Fungus3_archive_02, AddHazard);

    protected override void UnloadInternal() => Events.RemoveSceneChangeEdit(SceneNames.Fungus3_archive_02, AddHazard);

    private void AddHazard(Scene scene)
    {
        GameObject obj = new("HRT") { layer = (int)PhysLayers.HERO_DETECTOR };

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
