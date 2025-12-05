using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class CustomSpriteFlash : MonoBehaviour
{
    private SpriteRenderer? spriteRenderer;

    private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

    private Color flashColor;
    private float flashBase;
    private float flashTimer;
    private float flashDuration;

    internal void Flash(Color color, float start, float duration)
    {
        flashColor = color;
        flashBase = start;
        flashTimer = 0;
        flashDuration = duration;

        Update(0);
    }

    private void Update() => Update(Time.deltaTime);

    private void Update(float time)
    {
        flashTimer += time;
        float flashAmount = 0;
        if (flashTimer < flashDuration) flashAmount = Mathf.Lerp(flashBase, 0, flashTimer / flashDuration);

        MaterialPropertyBlock block = new();
        spriteRenderer?.GetPropertyBlock(block);
        block.SetFloat("_FlashAmount", flashAmount);
        block.SetColor("_FlashColor", flashColor);
        spriteRenderer?.SetPropertyBlock(block);
    }
}
