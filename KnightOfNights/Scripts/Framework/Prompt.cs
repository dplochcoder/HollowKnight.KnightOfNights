using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Framework;

internal class Prompt(PlayMakerFSM fsm)
{
    internal bool Visible
    {
        get => field;
        set
        {
            if (field != value) fsm.SendEvent(value ? "UP" : "DOWN");
            field = value;
        }
    } = false;

    public static Prompt Create(Vector3 pos, PromptType type)
    {
        var obj = Object.Instantiate(KnightOfNightsPreloader.Instance.ArrowPrompt, pos, Quaternion.identity);
        var fsm = obj.LocateMyFSM("Prompt Control");
        fsm.FsmVariables.GetFsmString("Prompt Name").Value = type.PromptName();

        return new(fsm);
    }
}

internal enum PromptType
{
    Inspect
}

internal static class PromptTypeExtensions
{
    internal static string PromptName(this PromptType self) => self switch { PromptType.Inspect => "Inspect", _ => throw self.InvalidEnum() };
}
