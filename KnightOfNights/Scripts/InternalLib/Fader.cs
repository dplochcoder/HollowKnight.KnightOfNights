using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class Fader : MonoBehaviour
{
    [ShimField] public Color StartColor;
    [ShimField] public Color EndColor;
    [ShimField] public float Delay;
    [ShimField] public float Duration;

    private SpriteRenderer? renderer;
    private float timer;

    private void Awake() => renderer = GetComponent<SpriteRenderer>();

    private void OnEnable() => timer = 0;

    private void Update()
    {
        timer += Time.deltaTime;

        var p = MathExt.Clamp((timer - Delay) / Duration, 0, 1);
        renderer?.color = Color.Lerp(StartColor, EndColor, p);
    }
}
