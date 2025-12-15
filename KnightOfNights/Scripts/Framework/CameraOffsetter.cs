using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.ModUtil;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class CameraOffsetter : MonoBehaviour
{
    [ShimField] public Vector2 Offset;
    [ShimField] public float OffsetSpeed;

    private float dist;
    private int detected = 0;

    private void ListenTo(HeroDetectorProxy proxy) => proxy.Listen(() => ++detected, () => --detected);

    private void Awake()
    {
        if (this.TryGetComponent<HeroDetectorProxy>(out var myProxy))
            ListenTo(myProxy);
        foreach (var proxy in this.GetComponentsInChildren<HeroDetectorProxy>())
            ListenTo(proxy);
    }

    private void Update() => dist.AdvanceFloatAbs(Time.deltaTime * OffsetSpeed, detected > 0 ? Offset.magnitude : 0);

    private bool ModifyCameraPosition(Vector3 pos, out Vector3 updatedPos)
    {
        updatedPos = pos;
        if (!enabled || dist <= 0f) return false;

        updatedPos += Offset.To3d().normalized * dist;
        return true;
    }

    private void OnEnable() => CameraPositionModifier.AddModifier(CameraModifierPhase.TARGET_BEFORE_LOCK, CameraPriorities.CAMERA_OFFSETTER, ModifyCameraPosition);

    private void OnDisable() => CameraPositionModifier.RemoveModifier(CameraModifierPhase.TARGET_BEFORE_LOCK, CameraPriorities.CAMERA_OFFSETTER, ModifyCameraPosition);
}
