using KnightOfNights.Scripts.SharedLib;
using UnityEngine;

namespace KnightOfNights.Scripts.Lib
{
    public class CaveBuilderNode : MonoBehaviour
    {
        public float Depth;

#if UNITY_EDITOR
        [UnityEditor.DrawGizmo(UnityEditor.GizmoType.InSelectionHierarchy)]
        private static void DrawDepth(Transform transform, UnityEditor.GizmoType gizmoType)
        {
            if (transform.gameObject.TryGetComponent<CaveBuilderNode>(out var node))
            {
                var style = new GUIStyle();
                style.fontSize = 14;
                style.fontStyle = FontStyle.Bold;
                UnityEditor.Handles.Label(transform.position, $"{node.Depth:0.00}", style);
            }
        }

        [UnityEditor.Callbacks.PostProcessScene]
        public static void DeleteMe() => UnityEditorShims.DeleteAll<CaveBuilderNode>();
#endif
    }
}
