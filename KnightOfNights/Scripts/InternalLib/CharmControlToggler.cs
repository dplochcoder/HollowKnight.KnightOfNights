using KnightOfNights.Scripts.SharedLib;
using KnightOfNights.Util;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class CharmControlToggler : MonoBehaviour
{
    [ShimField] public bool EnableWithAnyCharms;

    private readonly List<GameObject> children = [];
    private bool EnableChildren
    {
        get => field;
        set
        {
            if (field == value) return;

            field = value;
            children.ForEach(o => o.SetActive(value));
        }
    } = false;

    private void Awake()
    {
        children.AddRange(gameObject.Children());
        Update();
        children.ForEach(o => o.SetActive(EnableChildren));
    }

    private void Update() => EnableChildren = EnableWithAnyCharms == CharmIds.EquippedAnyCharmsBesidesVoidHeart();
}
