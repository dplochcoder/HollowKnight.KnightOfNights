using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using SFCore;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class ShadelessModule : AbstractModule<ShadelessModule>
{
    private static readonly FsmID deathAnimId = new("Hero Death", "Hero Death Anim");

    private const string NAME_KEY = "INV_SHADELESS_NAME";
    private const string DESC_KEY = "INV_SHADELESS_DESC";

    internal static void HookItemHelper()
    {
        EmbeddedSprite sprite = new("shadeless");
        ItemHelper.AddNormalItem(sprite.Value, nameof(IsShadeless), NAME_KEY, DESC_KEY);
    }

    public bool IsShadeless;

    protected override ShadelessModule Self() => this;

    public override void Initialize()
    {
        base.Initialize();

        Events.AddFsmEdit(deathAnimId, ModifyDeathAnim);
        Events.AddLanguageEdit(new(NAME_KEY), FillName);
        Events.AddLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook += HookIsShadeless;
    }

    public override void Unload()
    {
        Events.RemoveFsmEdit(deathAnimId, ModifyDeathAnim);
        Events.RemoveLanguageEdit(new(NAME_KEY), FillName);
        Events.RemoveLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook -= HookIsShadeless;

        base.Unload();
    }

    private void ModifyDeathAnim(PlayMakerFSM fsm)
    {
        fsm.GetState("Remove Geo").ClearActions();
        fsm.GetState("Limit Soul").ClearActions();

        var setShadeState = fsm.GetState("Set Shade");
        setShadeState.AddTransition("SKIP", "Save");
        setShadeState.AddFirstAction(new Lambda(() =>
        {
            fsm.FsmVariables.GetFsmGameObject("Self").Value = fsm.gameObject;
            fsm.SendEvent("SKIP");
        }));
    }

    private static void FillName(ref string value) => value = "Shadeless";

    private static void FillDesc(ref string value) => value = "In this world, the Knight leaves no shade. No geo is dropped upon death.";

    private bool HookIsShadeless(string name, bool orig) => name == nameof(IsShadeless) ? IsShadeless : orig;
}
