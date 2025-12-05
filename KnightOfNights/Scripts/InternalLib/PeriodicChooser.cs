using KnightOfNights.Scripts.SharedLib;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal class PeriodicChooser<T>(float min, float max, List<T> items)
{
    private readonly float min = min;
    private readonly float max = max;
    private readonly List<T> items = [.. items];

    private T choice = items.Choose();
    private float remaining = Random.Range(min, max);

    internal T Update(float time)
    {
        remaining -= time;
        while (remaining < 0)
        {
            choice = items.Choose();
            remaining += Random.Range(min, max);
        }

        return choice;
    }
}
