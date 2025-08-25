using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

[Shim]
public class PatchPlayMakerManager : MonoBehaviour
{
    private void Awake()
    {
        var go = Object.Instantiate(KnightOfNightsPreloader.Instance.PlayMaker);
        go.SetActive(true);
        go.name = "PlayMaker Unity 2D";
    }
}
