using Benchwarp;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.IC;
using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class BenchProxy : MonoBehaviour
{
    [ShimField] public string AreaName = "";
    [ShimField] public string MenuName = "";
    [ShimField] public Vector3 AdjustVector;

    private void Awake()
    {
        var sprite = GetComponent<SpriteRenderer>().sprite;
        var myBox = GetComponent<BoxCollider2D>();

        var bench = ObjectCache.GetNewBench();
        bench.tag = "RespawnPoint";
        bench.SetActive(true);

        bench.name = transform.parent.name + "-RespawnMarker";
        bench.transform.position = transform.position with { z = 0.01f };
        bench.transform.localScale = transform.localScale;
        bench.transform.localRotation = Quaternion.identity;
        bench.GetComponent<SpriteRenderer>().sprite = sprite;

        var box = bench.GetComponent<BoxCollider2D>();
        box.offset = myBox.offset;
        box.size  = myBox.size;

        var lit = bench.FindChild("Lit")!;
        lit.transform.localPosition = Vector3.zero;
        lit.GetComponent<SpriteRenderer>().sprite = sprite;

        var fsm = bench.LocateMyFSM("Bench Control");
        var vars = fsm.FsmVariables;
        vars.GetFsmBool("Tilter").Value = false;
        vars.GetFsmFloat("Tilt Amount").Value = 0;
        vars.GetFsmVector3("Adjust Vector").Value = AdjustVector;

        fsm.GetState("Rest Burst").AddFirstAction(new Lambda(() => BenchesModule.Get()?.VisitBench(AreaName, MenuName)));
        gameObject.SetActive(false);
    }
}
