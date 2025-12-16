using ItemChanger.Extensions;
using KnightOfNights.Scripts.Framework;
using KnightOfNights.Scripts.InternalLib;
using KnightOfNights.Scripts.Proxy;
using KnightOfNights.Scripts.SharedLib;
using PurenailCore.CollectionUtil;
using PurenailCore.ModUtil;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnightOfNights.Scripts.Summit;

[Shim]
internal class Binoculars : MonoBehaviour
{
    [ShimField] public GameObject? HudPrefab;

    [ShimField] public HeroDetectorProxy? Detector;
    [ShimField] public GameObject? CollidersParent;
    [ShimField] public Transform? CameraStart;
    [ShimField] public float CameraSpeed;

    private List<Collider2D> validRanges = [];
    private Prompt? prompt;

    private void Awake()
    {
        validRanges = [.. CollidersParent!.GetComponentsInChildren<Collider2D>()];

        var pos = transform.position;
        pos.y += 1.5f;
        prompt = Prompt.Create(pos, PromptType.Inspect);

        this.StartLibCoroutine(Run());
    }

    private bool CanInspect()
    {
        if (!Detector!.Detected()) return false;

        var heroController = HeroController.instance;
        if (!heroController.CanInput()) return false;

        var cState = HeroController.instance.cState;
        if (cState.attacking || cState.downAttacking || cState.upAttacking || cState.dashing) return false;
        if (!cState.onGround) return false;

        return true;
    }

    private IEnumerator<CoroutineElement> Run()
    {
        var heroController = HeroController.instance;
        var vignette = heroController.gameObject.FindChild("Vignette")!;
        var animator = heroController.gameObject.GetComponent<tk2dSpriteAnimator>();
        var inputHandler = InputHandler.Instance;
        var canvas = GameCameras.instance.hudCanvas;

        while (true)
        {
            yield return Coroutines.SleepUntil(() => CanInspect() && inputHandler.inputActions.up.WasPressed);

            inspectable = false;
            heroController.RelinquishControl();
            heroController.StopAnimationControl();

            yield return Coroutines.Sequence(MoveKnightToCenter());
            animator.Play("TurnToBG");

            var hudObj = Instantiate(HudPrefab!);
            hudObj.transform.parent = canvas.transform;
            hudObj.layer = canvas.layer;
            foreach (var child in hudObj.Children()) child.layer = canvas.layer;
            hudObj.transform.localPosition = new(8.71f, -6.6f);
            var hud = hudObj.GetComponent<BinocularHud>();

            Wrapped<bool> changeCamera = new(false);
            hud.OnRelocateCamera = () => changeCamera.Value = true;
            var hudAnimator = hudObj.GetComponent<Animator>();
            hudAnimator.runtimeAnimatorController = hud.FadeIn;
            yield return Coroutines.SleepUntil(() => changeCamera.Value);

            ActiveBinoculars = this;
            vignette.SetActive(false);
            activeCameraPos = ClampCameraPos(CameraStart!.position);

            yield return Coroutines.SleepSeconds(1);
            yield return Coroutines.SleepUntil(() => inputHandler.inputActions.attack.WasPressed);

            changeCamera.Value = false;
            hudAnimator.runtimeAnimatorController = hud.FadeOut;
            yield return Coroutines.SleepUntil(() => changeCamera.Value);
            hudObj.DestroyAfter(3);

            ActiveBinoculars = null;
            vignette.SetActive(true);
            animator.Play("TurnFromBG");
            yield return Coroutines.SleepUntil(() => !animator.Playing);

            heroController.StartAnimationControl();
            heroController.RegainControl();

            yield return Coroutines.SleepSeconds(0.5f);
            inspectable = true;
        }
    }

    private IEnumerator<CoroutineElement> MoveKnightToCenter()
    {
        var heroController = HeroController.instance;
        var knight = heroController.gameObject;
        var kx = knight.transform.position.x;
        var animator = knight.GetComponent<tk2dSpriteAnimator>();

        bool facingRight = knight.transform.localScale.x < 0;
        bool onRight = kx > transform.position.x;
        if (facingRight != !onRight)
        {
            animator.Play("Turn");
            yield return Coroutines.SleepUntil(() => !animator.Playing);

            if (facingRight) heroController.FaceLeft();
            else heroController.FaceRight();
        }

        animator.Play("Walk");
        var rigidbody = knight.GetComponent<Rigidbody2D>();
        var origSign = Mathf.Sign(transform.position.x - kx);
        rigidbody.velocity = new(origSign * 6, 0);

        yield return Coroutines.SleepUntil(() => Mathf.Sign(transform.position.x - knight.transform.position.x) != origSign);
        rigidbody.velocity = Vector3.zero;
        knight.transform.SetPositionX(transform.position.x);
    }

    private bool inspectable = true;
    private Vector2 activeCameraPos;

    private void Update()
    {
        prompt!.Visible = inspectable && Detector!.Detected();

        if (ActiveBinoculars == this)
        {
            var vec = InputHandler.Instance.inputActions.moveVector;
            Vector2 dir = new(vec.X, vec.Y);
            if (dir.sqrMagnitude > 0.01f)
            {
                var newPos = activeCameraPos + dir.normalized * CameraSpeed * Time.deltaTime;
                activeCameraPos = ClampCameraPos(newPos);
            }
        }
    }

    private Vector3 ClampCameraPos(Vector2 pos)
    {
        Vector2 closest = pos;
        float dist = Mathf.Infinity;
        foreach (var range in validRanges)
        {
            var cClosest = range.ClosestPoint(pos);
            var cDist = (cClosest - pos).sqrMagnitude;
            if (cDist < dist)
            {
                dist = cDist;
                closest = cClosest;
            }
        }

        return closest;
    }

    private static Binoculars? ActiveBinoculars;

    private static bool ApplyBinoculars(Vector3 pos, out Vector3 newPos)
    {
        newPos = pos;
        if (ActiveBinoculars == null) return false;

        newPos.x = ActiveBinoculars.activeCameraPos.x;
        newPos.y = ActiveBinoculars.activeCameraPos.y;
        return true;
    }

    static Binoculars() => CameraPositionModifier.AddModifier(CameraModifierPhase.FINAL_POSITON, 1f, ApplyBinoculars);
}

[Shim]
internal class BinocularHud : MonoBehaviour
{
    [ShimField] public RuntimeAnimatorController? FadeIn;
    [ShimField] public RuntimeAnimatorController? FadeOut;

    internal Action? OnRelocateCamera;

    [ShimMethod] public void RelocateCamera() => OnRelocateCamera?.Invoke();
}
