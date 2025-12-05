using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class Fader : MonoBehaviour
{
    [ShimField] public Color StartColor;
    [ShimField] public Color EndColor;
    [ShimField] public float Duration;

    private SpriteRenderer? renderer;
    private float timer;

    private void Awake() => renderer = GetComponent<SpriteRenderer>();

    private void OnEnable() => timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > Duration) timer = Duration;

        if (renderer != null) renderer.color = Color.Lerp(StartColor, EndColor, timer / Duration);
    }
}
