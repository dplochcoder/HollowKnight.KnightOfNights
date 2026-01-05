using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Prefab Replacement Tool")]
class PrefabReplacementTool : EditorTool
{
    private static GameObject prefab;

    public override GUIContent toolbarIcon => base.toolbarIcon;

    public override bool IsAvailable() => true;

    public override void OnActivated() => EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

    public override void OnWillBeDeactivated() => EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;

    public override void OnToolGUI(EditorWindow window) => HandleKeyEvent();

    private void OnHierarchyGUI(int instanceId, Rect selectionRect) => HandleKeyEvent();

    private void HandleKeyEvent()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (e.keyCode == KeyCode.P)
                    {
                        var objects = Selection.gameObjects.ToList();
                        List<Transform> next = new List<Transform>();
                        foreach (var obj in objects)
                        {
                            var replacement = (GameObject)PrefabUtility.InstantiatePrefab(prefab, obj.transform.parent);
                            replacement.transform.localPosition = obj.transform.localPosition;
                            replacement.transform.localScale = obj.transform.localScale;
                            replacement.transform.localRotation = obj.transform.localRotation;
                            replacement.name = obj.name;

                            DestroyImmediate(obj);
                            next.Add(replacement.transform);
                        }

                        Selection.activeTransform = next[0];
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.Escape)
                    {
                        e.Use();
                        Deactivate();
                    }
                    break;
                }
            default:
                break;
        }
    }

    private static System.Type previousToolType;

    private static void Deactivate()
    {
        if (previousToolType != null) ToolManager.SetActiveTool(previousToolType);
        else ToolManager.RestorePreviousPersistentTool();
    }

    [Shortcut("Prefab Replacement Shortcut", KeyCode.O)]
    public static void ActivateTool()
    {
        if (ToolManager.activeToolType == typeof(PrefabReplacementTool))
        {
            Deactivate();
            return;
        }

        var filtered = Selection.gameObjects.Where(go => PrefabUtility.IsAnyPrefabInstanceRoot(go)).ToList();
        if (filtered.Count == 1)
        {
            prefab = PrefabUtility.GetCorrespondingObjectFromSource(filtered[0]);
            if (prefab != null)
            {
                previousToolType = ToolManager.activeToolType;
                ToolManager.SetActiveTool<PrefabReplacementTool>();
                return;
            }
        }
        Debug.Log("Couldn't start PrefabReplacementTool");
    }
}
