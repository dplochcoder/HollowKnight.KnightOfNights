using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal class MarkothShieldWave : MonoBehaviour
{
    private readonly List<GameObject> shields = [];

    private ShieldCycloneStats? stats;

    private bool flipped;
    private float expansion;
    private float expansionSpeed;
    private float rotation;
    private float rotationSpeed;

    public static MarkothShieldWave Spawn(ShieldCycloneStats stats, float rotationOffset, Vector3 pos, bool flipped)
    {
        GameObject root = new("Wave");
        root.transform.position = pos;
        KnightOfNightsPreloader.Instance.MageTeleportClip?.PlayAtPosition(pos, 0.85f);

        var wave = root.AddComponent<MarkothShieldWave>();
        wave.flipped = flipped;
        wave.stats = stats;
        wave.rotation = rotationOffset;
        wave.rotationSpeed = stats.RotationSpeedStart * (flipped ? -1 : 1);
        wave.expansion = stats.ExpansionStart;
        wave.expansionSpeed = stats.ExpansionStartSpeed;
        for (int i = 0; i < stats.ShieldsPerWave; i++)
        {
            var shield = Instantiate(KnightOfNightsPreloader.Instance.MarkothShield!);
            shield.transform.SetParent(root.transform, true);
            shield.transform.localRotation = Quaternion.Euler(0, 0, 180f + (i * 360f) / stats.ShieldsPerWave);
            wave.shields.Add(shield);
        }
        wave.SetPositions();

        root.SetActive(true);
        return wave;
    }

    internal void Update()
    {
        float s = flipped ? -1 : 1;
        expansion.SimpleAccelerate(ref expansionSpeed, stats!.ExpansionTopSpeed, stats.ExpansionAccel, Time.deltaTime);
        rotation.SimpleDecelerate(ref rotationSpeed, stats.RotationSpeedMinimum, stats.RotationSpeedDecel, Time.deltaTime);

        SetPositions();
        if (expansion > stats.ExpansionLimit) Despawn();
    }

    internal void Despawn() => Destroy(gameObject);

    private void SetPositions()
    {
        transform.localRotation = Quaternion.Euler(0, 0, rotation);
        for (int i = 0; i < shields.Count; i++)
            shields[i].transform.localPosition = Quaternion.Euler(0, 0, (i * 360f) / shields.Count) * new Vector3(expansion, 0);
    }
}
