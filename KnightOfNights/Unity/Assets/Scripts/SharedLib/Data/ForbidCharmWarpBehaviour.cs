using KnightOfNights.Scripts.InternalLib;
using System;

namespace KnightOfNights.Scripts.SharedLib.Data
{
    [Serializable]
    public class ForbidCharmWarp
    {
        public bool WholeScene;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
    }

    public class ForbidCharmWarpBehaviour : SceneDataProvider
    {
        public ForbidCharmWarp Data;

        public override object GetSceneData() => Data;
    }
}
