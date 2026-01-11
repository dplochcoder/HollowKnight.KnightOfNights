using SFCore.Utils;
using System.Linq;
using UnityEngine;

namespace KnightOfNights.Scripts.InternalLib;

internal static class AreaTitleUtil
{
    public static void Spawn(string key, string visitedBool = "")
    {
        var obj = Object.Instantiate(KnightOfNightsPreloader.Instance.AreaTitleController!);
        var fsm = obj.LocateMyFSM("Area Title Controller");

        var vars = fsm.FsmVariables;
        vars.GetFsmString("Area Event").Value = key;
        vars.GetFsmBool("Display Right").Value = false;
        vars.GetFsmBool("Sub Area").Value = false;
        vars.GetFsmFloat("Unvisited Pause").Value = 2;
        vars.GetFsmFloat("Visited Pause").Value = 1;
        vars.GetFsmGameObject("Area Title").Value = GameObject.Find("Area Title");

        // Define private Area object
        var areaType = typeof(AreaTitleController).GetNestedType("Area", System.Reflection.BindingFlags.NonPublic);
        var con = areaType.GetConstructor([typeof(string), typeof(int), typeof(bool), typeof(string)]);
        var areaObj = con.Invoke([key, -1, false, visitedBool]);

        // Add new areas
        var atc = obj.GetComponent<AreaTitleController>();
        var atcList = atc.GetAttr<AreaTitleController, object>("areaList");
        var addMethod = atcList.GetType().GetMethods().Where(mi => mi.Name == "Add" && mi.GetParameters().Length == 1).FirstOrDefault();
        addMethod.Invoke(atcList, [areaObj]);

        obj.SetActive(true);
    }
}
