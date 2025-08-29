using ItemChanger;
using Modding;
using SFCore;

namespace KnightOfNights.IC;

[PlandoSubmodule]
internal class StoneKeyModule : AbstractModule<StoneKeyModule>
{
    private const string NAME_KEY = "INV_STONE_KEY_NAME";
    private const string DESC_KEY = "INV_STONE_KEY_DESC";

    internal static void HookItemHelper()
    {
        EmbeddedSprite sprite = new("stonekey");
        ItemHelper.AddNormalItem(sprite.Value, nameof(HasStoneKey), NAME_KEY, DESC_KEY);
    }

    public bool HasStoneKey;

    protected override StoneKeyModule Self() => this;

    private static void FillName(ref string value) => value = "Stone Key";

    private static void FillDesc(ref string value) => value = "An ancient stone key of cursed origin. It leaks void.";

    private bool HookHasStoneKey(string name, bool orig) => name == nameof(HasStoneKey) ? HasStoneKey : orig;

    // TODO: Door

    public override void Initialize()
    {
        base.Initialize();

        Events.AddLanguageEdit(new(NAME_KEY), FillName);
        Events.AddLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook += HookHasStoneKey;
    }

    public override void Unload()
    {
        Events.RemoveLanguageEdit(new(NAME_KEY), FillName);
        Events.RemoveLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook -= HookHasStoneKey;

        base.Unload();
    }
}
