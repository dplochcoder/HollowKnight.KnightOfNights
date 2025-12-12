using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using Modding;
using SFCore;
using System.Collections.Generic;

namespace KnightOfNights.IC;

internal enum FluteNote
{
    Left,
    Right
}

[PlandoSubmodule]
internal class RevekSongModule : AbstractModule<RevekSongModule>
{
    private const string NAME_KEY = "INV_REVEK_SONG_NAME";
    private const string DESC_KEY = "INV_REVEK_SONG_DESC";

    internal static void HookItemHelper()
    {
        EmbeddedSprite sprite = new("reveksong");
        ItemHelper.AddNormalItem(sprite.Value, nameof(HasRevekSong), NAME_KEY, DESC_KEY);
    }

    private static readonly FsmID dreamNailId = new("Knight", "Dream Nail");

    public bool HasRevekSong;

    protected override RevekSongModule Self() => this;

    private static void FillName(ref string value) => value = "Revek Song";

    private static void FillDesc(ref string value) => value = "Press left or right three times while swinging the Dream Nail. Revek will answer the call until parried three times in a row.<br><br>Successful parries restore health and soul, and expedite the next strike. Directions given can be mixed and determine in which way Revek shall strike.<br><br>Go forth with the warrior's blessing. RandoMapMod is your friend.";

    private bool HookHasRevekSong(string name, bool orig) => name == nameof(HasRevekSong) ? HasRevekSong : orig;

    private void FixRevekSong(Transition t) => RevekSongSummon.revekActive = false;

    public override void Initialize()
    {
        base.Initialize();

        PlandoModule.OnEveryFrame += UpdateRevekSong;
        Events.AddFsmEdit(dreamNailId, HookRevekSong);
        Events.AddLanguageEdit(new(NAME_KEY), FillName);
        Events.AddLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook += HookHasRevekSong;
        Events.OnBeginSceneTransition += FixRevekSong;
    }

    public override void Unload()
    {
        PlandoModule.OnEveryFrame -= UpdateRevekSong;
        Events.RemoveFsmEdit(dreamNailId, HookRevekSong);
        Events.RemoveLanguageEdit(new(NAME_KEY), FillName);
        Events.RemoveLanguageEdit(new(DESC_KEY), FillDesc);
        ModHooks.GetPlayerBoolHook -= HookHasRevekSong;
        Events.OnBeginSceneTransition -= FixRevekSong;

        base.Unload();
    }

    private void HookRevekSong(PlayMakerFSM fsm)
    {
        fsm.GetState("Start").AddFirstAction(new Lambda(StartSessopm));
        fsm.GetState("End").AddFirstAction(new Lambda(FinishSession));
        fsm.GetState("Allow Dream Gate").AddFirstAction(new Lambda(CancelSession));
        fsm.GetState("Cancel").AddFirstAction(new Lambda(CancelSession));
        fsm.GetState("Regain Control").AddFirstAction(new Lambda(CancelSession));
        fsm.GetState("Set Charge Start").AddFirstAction(new Lambda(CancelSession));
        fsm.GetState("Warp Charge Start").AddFirstAction(new Lambda(CancelSession));
    }

    private bool inSession;
    private readonly HashSet<FluteNote> activeNotes = [];
    private readonly List<FluteNote> finishedNotes = [];

    private void StartSessopm()
    {
        if (!HasRevekSong) return;

        inSession = true;
    }

    private void CancelSession()
    {
        inSession = false;
        activeNotes.Clear();
        finishedNotes.Clear();
    }

    private void UpdateNote(InControl.PlayerAction input, FluteNote note)
    {
        if (finishedNotes.Count >= 3) return;

        if (input.WasPressed) activeNotes.Add(note);
        else if (input.WasReleased && activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
            finishedNotes.Add(note);
        }
    }

    private void UpdateRevekSong()
    {
        if (!inSession) return;

        var actions = InputHandler.Instance.inputActions;

        // TODO: Effects
        UpdateNote(actions.left, FluteNote.Left);
        UpdateNote(actions.right, FluteNote.Right);
    }

    private void FinishSession() => RevekSongSummon.Summon([.. finishedNotes]);
}
