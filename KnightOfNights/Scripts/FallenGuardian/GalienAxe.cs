using HutongGames.PlayMaker.Actions;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using UnityEngine;

namespace KnightOfNights.Scripts.FallenGuardian;

internal class GalienAxe : MonoBehaviour
{
    private float timeToAntic;

    internal event System.Action<Vector3>? OnDespawn;

    private bool destroyed = false;

    internal void Despawn()
    {
        if (destroyed) return;

        destroyed = true;
        Destroy(gameObject);
    }

    internal static GalienAxe Spawn(AxeHopscotchStats stats, BoxCollider2D arena, Vector2 pos)
    {
        var obj = Instantiate(KnightOfNightsPreloader.Instance.GalienAxe!, pos, Quaternion.identity);
        var bounds = arena.bounds;

        var ctrl = obj.LocateMyFSM("Control");
        ctrl.FsmVariables.GetFsmVector3("Float Vector").Value = new(0, bounds.min.y + stats.AxeFloatHeight);

        var emergeState = ctrl.GetState("Emerge");
        emergeState.GetFirstActionOfType<iTweenRotateTo>().time = stats.AxeEmergeTime;
        emergeState.GetFirstActionOfType<iTweenMoveTo>().time = stats.AxeEmergeTime;

        var startSpinState = ctrl.GetState("Start Spin");
        startSpinState.GetFirstActionOfType<FloatAdd>().add = -1800 / stats.AxeSpinTime;
        startSpinState.GetFirstActionOfType<Wait>().time = stats.AxeSpinTime;
        startSpinState.GetFirstActionOfType<EaseFloat>().time = stats.AxeSpinTime;
        startSpinState.GetFirstActionOfType<FadeAudio>().time = stats.AxeSpinTime / 2;

        ctrl.GetState("Decel").GetFirstActionOfType<FadeAudio>().time = stats.AxeDecelTime;

        var attack = obj.LocateMyFSM("Attack");

        var vars = attack.FsmVariables;
        vars.GetFsmFloat("Floor Y").Value = bounds.min.y + 1.88f;
        vars.GetFsmFloat("Ran Float").Value = stats.AxeBounceSpeed;
        vars.GetFsmFloat("Slam Y").Value = bounds.min.y + 2.24f;
        vars.GetFsmFloat("Wall L X").Value = bounds.min.x + 2.1f;
        vars.GetFsmFloat("Wall R X").Value = bounds.max.x - 2.1f;

        var anticState = attack.GetState("Antic");
        var move = anticState.GetFirstActionOfType<iTweenMoveBy>();
        move.time = stats.AxeAnticTime;
        move.vector.Value = new(0, stats.AxeAnticRiseY, 0);
        anticState.RemoveActionsOfType<FindGameObject>();

        var chaseState = attack.GetState("Chase");
        var chaseAction = chaseState.GetFirstActionOfType<ChaseObjectGround>();
        chaseAction.speedMax = stats.AxeMaxSpeed;
        chaseAction.acceleration = stats.AxeAccel;
        chaseState.GetFirstActionOfType<AccelerateVelocity>().yAccel = -stats.AxeGravity;
        var tests = chaseState.GetActionsOfType<FloatTestToBool>();
        tests[0].float2 = stats.AxeTimer;
        tests[1].float2 = bounds.min.y + 7.25f;

        var floorBounceState = attack.GetState("Floor Bounce");
        floorBounceState.RemoveActionsOfType<RandomFloat>();
        floorBounceState.AddFirstAction(new Lambda(() => SpawnPrefab(stats.SnowImpactPrefab!, new(obj.transform.position.x, bounds.min.y))));

        attack.GetState("Wall L").AddFirstAction(new Lambda(() => SpawnPrefab(stats.WallImpactLPrefab!, new(bounds.min.x, obj.transform.position.y), 1f)));
        attack.GetState("Wall R").AddFirstAction(new Lambda(() => SpawnPrefab(stats.WallImpactLPrefab!, new(bounds.max.x, obj.transform.position.y), -1f)));
        attack.GetState("Wall Y").ClearActions();

        var decelState = attack.GetState("Decel");
        decelState.GetFirstActionOfType<Wait>().time = stats.AxeDecelTime;
        decelState.GetFirstActionOfType<FloatAddV2>().add = 800 / stats.AxeDecelTime;

        var axe = obj.AddComponent<GalienAxe>();
        axe.timeToAntic = stats.WaitAnticAfterSpawn;
        attack.GetState("Recel").AddFirstAction(new Lambda(() => axe.Despawn()));

        obj.SetActive(true);
        return axe;
    }

    private float bounceTime;

    private static void SpawnPrefab(GameObject prefab, Vector2 pos, float scale = 1f)
    {
        var obj = prefab.Spawn(pos);
        obj.transform.localScale = new(scale, 1, 1);
    }

    private void OnDestroy()
    {
        destroyed = true;
        OnDespawn?.Invoke(transform.position);
    }

    private PlayMakerFSM? ctrl;
    private PlayMakerFSM? attack;

    private void Awake()
    {
        ctrl = gameObject.LocateMyFSM("Control");
        attack = gameObject.LocateMyFSM("Attack");
    }

    private void Update()
    {
        bounceTime += Time.deltaTime;
        if (ctrl!.ActiveStateName == "Init") ctrl.SendEvent("READY");

        if (timeToAntic <= 0) return;
        timeToAntic -= Time.deltaTime;
        if (timeToAntic <= 0) attack!.SendEvent("HAMMER ATTACK");
    }
}
