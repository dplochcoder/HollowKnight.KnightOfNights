using KnightOfNights.IC;
using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
internal class DreamgateSetBlocker : MonoBehaviour
{
    [ShimField] public HeroDetectorProxy? Proxy;

    private void Awake() => Proxy?.Listen(() => DreamGateControllerModule.Get()?.AddSetBlocker(), () => DreamGateControllerModule.Get()?.RemoveSetBlocker());
}
