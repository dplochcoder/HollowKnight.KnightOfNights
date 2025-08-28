using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using KnightOfNights.Songs;
using System.Collections.Generic;

namespace KnightOfNights.IC;

internal enum FluteNote
{
    Up,
    Left,
    Right,
    Down
}

internal interface IFluteSong
{
    string Name();

    List<FluteNote> Notes();

    void Summon();
}

[PlandoSubmodule]
internal class DreamFluteModule : AbstractModule<DreamFluteModule>
{
    private static FsmID dreamNailId = new("Knight", "Dream Nail");

    private static List<IFluteSong> AllSongs = [new SongOfRevek()];

    public bool HasDreamFlute;
    public HashSet<string> LearnedSongs = [];

    protected override DreamFluteModule Self() => this;

    internal void GiveAllSongs() => AllSongs.ForEach(s => LearnedSongs.Add(s.Name()));

    public override void Initialize()
    {
        base.Initialize();

        PlandoModule.OnEveryFrame += UpdateDreamFlute;
        Events.AddFsmEdit(dreamNailId, HookDreamFlute);
    }

    public override void Unload()
    {
        PlandoModule.OnEveryFrame -= UpdateDreamFlute;
        Events.RemoveFsmEdit(dreamNailId, HookDreamFlute);

        base.Unload();
    }

    private void HookDreamFlute(PlayMakerFSM fsm)
    {
        fsm.GetState("Start").AddFirstAction(new Lambda(StartFluteSession));
        fsm.GetState("End").AddFirstAction(new Lambda(FinishFluteSession));
        fsm.GetState("Allow Dream Gate").AddFirstAction(new Lambda(CancelFluteSession));
        fsm.GetState("Cancel").AddFirstAction(new Lambda(CancelFluteSession));
        fsm.GetState("Regain Control").AddFirstAction(new Lambda(CancelFluteSession));
        fsm.GetState("Set Charge Start").AddFirstAction(new Lambda(CancelFluteSession));
        fsm.GetState("Warp Charge Start").AddFirstAction(new Lambda(CancelFluteSession));

        var dgateState = fsm.GetState("Dream Gate?");
        dgateState.RemoveActionsOfType<ListenForDown>();
        dgateState.RemoveActionsOfType<ListenForUp>();
        dgateState.InsertAction(new Lambda(() => MaybeDreamGate(fsm)), 1);
    }

    private bool inFluteSession;
    private HashSet<FluteNote> activeNotes = [];
    private List<FluteNote> finishedNotes = [];
    private IFluteSong? playedSong;

    private void StartFluteSession()
    {
        if (!HasDreamFlute) return;

        inFluteSession = true;
    }

    private void CancelFluteSession()
    {
        inFluteSession = false;
        activeNotes.Clear();
        finishedNotes.Clear();
        playedSong = null;
    }

    private void MaybeDreamGate(PlayMakerFSM fsm)
    {
        if (HasDreamFlute && (finishedNotes.Count > 0 || activeNotes.Count > 1)) return;

        var actions = InputHandler.Instance.inputActions;

        if (actions.down.IsPressed) fsm.SendEvent("SET");
        else if (actions.up.IsPressed) fsm.SendEvent("WARP");
    }

    private void UpdateNote(InControl.PlayerAction input, FluteNote note)
    {
        if (input.WasPressed) activeNotes.Add(note);
        else if (input.WasReleased && activeNotes.Contains(note))
        {
            activeNotes.Remove(note);
            finishedNotes.Add(note);
        }
    }

    private bool PlayedSong(List<FluteNote> song)
    {
        if (finishedNotes.Count != song.Count) return false;

        for (int i = 0; i < song.Count; i++) if (finishedNotes[i] != song[i]) return false;
        return true;
    }

    private void UpdateDreamFlute()
    {
        if (!inFluteSession || playedSong != null) return;

        var actions = InputHandler.Instance.inputActions;

        // TODO: Effects
        UpdateNote(actions.up, FluteNote.Up);
        UpdateNote(actions.left, FluteNote.Left);
        UpdateNote(actions.right, FluteNote.Right);
        UpdateNote(actions.down, FluteNote.Down);

        foreach (var song in AllSongs)
        {
            if (!LearnedSongs.Contains(song.Name())) continue;
            if (PlayedSong(song.Notes()))
            {
                playedSong = song;
                break;
            }
        }
    }

    private void FinishFluteSession() => playedSong?.Summon();
}
