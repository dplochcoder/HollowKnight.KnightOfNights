using Benchwarp;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.IC;
using KnightOfNights.Scripts.SharedLib;
using SFCore.Utils;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

[Shim]
internal class BenchProxy : MonoBehaviour
{
    [ShimField] public string AreaName = "";
    [ShimField] public string MenuName = "";

    private void Awake()
    {
        var sprite = GetComponent<SpriteRenderer>().sprite;
        var myBox = GetComponent<BoxCollider2D>();

        var bench = ObjectCache.GetNewBench();
        bench.name = transform.parent.name;
        bench.transform.position = transform.position;
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
        vars.GetFsmVector3("Adjust Vector").Value = new(0, 0.1f, 0);

        fsm.GetFsmState("Rest Burst").AddFirstAction(new Lambda(() => BenchesModule.Get()?.VisitBench(AreaName, MenuName)));

        Destroy(gameObject);
    }
}
