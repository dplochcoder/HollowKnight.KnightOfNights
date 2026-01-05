using ItemChanger.Extensions;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Proxy;

[Shim]
[RequireComponent(typeof(BoxCollider2D))]
internal class GrassProxy : MonoBehaviour
{
    [ShimField] public SpriteRenderer? GrassPreview;
    [ShimField] public Material? GrassParticles;
    [ShimField("25")] public int NumParticles;

    [ShimField] public float HeightOffset;
    [ShimField] public float SwayAmount;
    [ShimField] public float SwayAmountVariance;
    [ShimField] public float SwaySpeed;
    [ShimField] public float SwaySpeedVariance;

    private void Awake()
    {
        var obj = Instantiate(KnightOfNightsPreloader.Instance.Grass, transform.position, transform.rotation)!;
        obj.transform.localScale = transform.localScale;
        obj.SetActive(true);

        var box = obj.GetComponent<BoxCollider2D>();
        var myBox = gameObject.GetComponent<BoxCollider2D>();
        box.offset = myBox.offset;
        box.size = myBox.size;

        var grassObj = obj.FindChild("grass_03")!;
        grassObj.transform.localPosition = GrassPreview!.transform.localPosition;
        grassObj.transform.localScale = GrassPreview.transform.localScale;
        grassObj.transform.localRotation = GrassPreview.transform.localRotation;
        grassObj.AddComponent<SpriteFixer>().Sprite = GrassPreview.GetComponent<SpriteRenderer>().sprite;

        var renderer = grassObj.GetComponent<SpriteRenderer>();
        renderer.color = GrassPreview.GetComponent<SpriteRenderer>().color;

        MaterialPropertyBlock block = new();
        renderer.GetPropertyBlock(block);
        block.SetFloat("_HeightOffset", HeightOffset);
        block.SetFloat("_SwayAmount", SwayAmount + Random.Range(-SwayAmountVariance, SwayAmountVariance));
        block.SetFloat("_SwaySpeed", SwaySpeed + Random.Range(-SwaySpeedVariance, SwaySpeedVariance));
        renderer.SetPropertyBlock(block);

        obj.FindChild("grass_03_death")!.GetComponent<SpriteRenderer>().sprite = null;

        var deathObj = obj.FindChild("Green Grass A")!;
        var deathParticles = deathObj.GetComponent<ParticleSystem>();
        var emission = deathParticles.emission;
        emission.rateOverTime = NumParticles / deathParticles.main.duration;

        GrassParticles!.shader = Shader.Find("Sprites/Default");
        deathObj.GetComponent<ParticleSystemRenderer>().material = GrassParticles;

        gameObject.SetActive(false);
    }
}

internal class SpriteFixer : MonoBehaviour
{
    internal Sprite? Sprite;

    private SpriteRenderer? spriteRenderer;

    private void Awake() => spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

    private void LateUpdate() => spriteRenderer!.sprite = Sprite;
}
