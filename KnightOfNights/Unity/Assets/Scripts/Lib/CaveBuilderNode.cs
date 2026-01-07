using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Lib
{
    public class CaveBuilderNode : MonoBehaviour
    {
        public float Depth;

#if UNITY_EDITOR
        [UnityEditor.Callbacks.PostProcessScene]
        public static void DeleteMe() => UnityEditorShims.DeleteAll<CaveBuilderNode>();
#endif
    }
}
