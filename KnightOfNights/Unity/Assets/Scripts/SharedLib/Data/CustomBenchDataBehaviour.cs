using KnightOfNights.Scripts.InternalLib;
using System;

namespace KnightOfNights.Scripts.SharedLib.Data
{
    [Serializable]
    public class CustomBenchData
    {
        public string RespawnMarkerName = "";
        public string AreaName = "";
        public string MenuName = "";
    }

    public class CustomBenchDataBehaviour : SceneDataProvider
    {
        public CustomBenchData Data = new CustomBenchData();

        public override object GetSceneData()
        {
            Data.RespawnMarkerName = name;
            return Data;
        }
    }
}
