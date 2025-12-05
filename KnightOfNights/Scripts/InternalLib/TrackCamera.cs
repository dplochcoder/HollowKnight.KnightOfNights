using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class TrackCamera : MonoBehaviour
{
    [ShimField] public Vector3 offset;

    private void LateUpdate()
    {
        var pos = GameManager.instance?.cameraCtrl?.transform.position;
        if (!pos.HasValue) return;

        transform.position = pos.Value + offset;
    }
}
