using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class AreaTitleUtil
{
    public static void Spawn(string key, string visitedBool = "")
    {
        bool visited = visitedBool == "" || PlayerData.instance.GetBool(visitedBool);
        if (!visited) PlayerData.instance.SetBool(visitedBool, true);

        var areaTitle = AreaTitle.instance;

        GameObject obj = new();
        obj.DoAfter(() =>
        {
            areaTitle.gameObject.SetActive(true);

            var fsm = areaTitle.gameObject.GetComponent<PlayMakerFSM>();
            fsm.FsmVariables.GetFsmBool("Visited").Value = visited;
            fsm.FsmVariables.GetFsmBool("NPC Title").Value = false;
            fsm.FsmVariables.GetFsmString("Area Event").Value = key;
        }, visited ? 1 : 2);
    }
}
