using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace KnightOfNights.Util;

internal static class Events
{
    private static readonly HashSet<Action<Scene>> nextSceneCbs = [];
    internal static event Action<Scene> OnNextSceneChange
    {
        add => nextSceneCbs.Add(value);
        remove => nextSceneCbs.Remove(value);
    }

    static Events()
    {
        ItemChanger.Events.OnSceneChange += scene =>
        {
            List<Action<Scene>> cbs = [.. nextSceneCbs];
            nextSceneCbs.Clear();

            foreach (var action in cbs)
            {
                try { action(scene); }
                catch (Exception ex) { KnightOfNightsMod.LogError($"Error: {ex}"); }
            }
        };
    }
}
