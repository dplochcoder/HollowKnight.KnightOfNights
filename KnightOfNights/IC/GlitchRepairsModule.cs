using GlobalEnums;
using ItemChanger;
using ItemChanger.Extensions;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KnightOfNights.IC;

// Disables WCS (Wall-cling storage) glitch; easy to activate with wind, buggy.
internal class GlitchRepairsModule : AbstractModule<GlitchRepairsModule>
{
    private static readonly List<FsmID> spellIds = [new("Fireball(Clone)", "Fireball Control"), new("Scr Heads 2", "FSM")];

    protected override GlitchRepairsModule Self() => this;

    protected override void InitializeInternal()
    {
        On.HeroController.CanWallJump += OverrideCanWallJump;
        spellIds.ForEach(id => Events.AddFsmEdit(id, DisableSpellPogos));
    }

    protected override void UnloadInternal()
    {
        On.HeroController.CanWallJump -= OverrideCanWallJump;
        spellIds.ForEach(id => Events.RemoveFsmEdit(id, DisableSpellPogos));
    }

    private static readonly MethodInfo checkStillTouchingWall = typeof(HeroController).GetMethod("CheckStillTouchingWall", BindingFlags.Instance | BindingFlags.NonPublic);

    private static bool CheckStillTouchingWall(HeroController self, CollisionSide collisionSide, bool checkTop = false) => (bool)checkStillTouchingWall.Invoke(self, [collisionSide, checkTop]);

    internal static bool FixBugs() => GameManager.instance.sceneName.StartsWith("Summit_");

    // Fix WCS.
    private static bool OverrideCanWallJump(On.HeroController.orig_CanWallJump orig, HeroController self)
    {
        if (!FixBugs()) return orig(self);

        var pd = PlayerData.instance;
        var cstate = self.cState;
        if (pd.GetBool(nameof(pd.hasWalljump)) && !cstate.touchingNonSlider && (cstate.wallSliding || (cstate.touchingWall && !cstate.onGround)) && !CheckStillTouchingWall(self, CollisionSide.left) && !CheckStillTouchingWall(self, CollisionSide.right))
            return false;

        return orig(self);
    }

    private static void DisableSpellPogos(PlayMakerFSM fsm) => fsm.gameObject.GetOrAddComponent<ConditionalNonBouncer>();
}

internal class ConditionalNonBouncer : MonoBehaviour
{
    private NonBouncer? nonBouncer;

    private void Awake()
    {
        nonBouncer ??= gameObject.GetOrAddComponent<NonBouncer>();
        nonBouncer.enabled = GlitchRepairsModule.FixBugs();
    }

    private void Update() => nonBouncer!.enabled = GlitchRepairsModule.FixBugs();
}
