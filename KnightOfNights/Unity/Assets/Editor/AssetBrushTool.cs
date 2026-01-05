using KnightOfNights.Scripts.Lib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.ShortcutManagement;
using UnityEngine;

[EditorTool("Asset Brush")]
class AssetBrushTool : EditorTool
{
    private static AssetBrush previousBrush;
    private static int previousGroup;
    private static int previousInstance;

    private AssetBrush brush;
    private int group;
    private int instance;

    private readonly Stack<GameObject> history = new Stack<GameObject>();
    private GameObject selection;

    public override GUIContent toolbarIcon => base.toolbarIcon;

    public override bool IsAvailable() => true;

    public override void OnActivated()
    {
        base.OnActivated();

        brush = previousBrush;
        group = previousGroup;
        instance = previousInstance;

        UpdateSelection(true);
    }

    private Vector2 lastMouseMove;
    private bool dragging = false;
    private Quaternion origQuat;
    private Vector2 origRadius;

    public override void OnToolGUI(EditorWindow window)
    {
        bool isSceneView = window is SceneView;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (e.keyCode == KeyCode.Space)
                    {
                        UpdateSelection(false);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.W)
                    {
                        if (++group == brush.Groups.Count) group = 0;
                        Debug.Log("HI WE HERE DOING THING");
                        UpdateSelection(true);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.S)
                    {
                        if (--group == -1) group = brush.Groups.Count - 1;
                        UpdateSelection(true);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.Q)
                    {
                        if (--instance == -1) instance = brush.Groups[group].Instances.Count - 1;
                        UpdateSelection(true);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.E)
                    {
                        if (++instance == brush.Groups[group].Instances.Count) instance = 0;
                        UpdateSelection(true);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.F)
                    {
                        var scale = selection.transform.localScale;
                        scale.x *= -1;
                        selection.transform.localScale = scale;
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.R)
                    {
                        var quat = selection.transform.localRotation;
                        var euler = quat.eulerAngles;
                        euler.z += 90;
                        quat.eulerAngles = euler;
                        selection.transform.localRotation = quat;
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.Z)
                    {
                        if (history.Count > 0) DestroyImmediate(history.Pop());
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.Escape)
                    {
                        while (history.Count > 0) DestroyImmediate(history.Pop());
                        e.Use();

                        Deactivate();
                    }
                    break;
                }
            case EventType.MouseMove:
                {
                    dragging = false;
                    if (isSceneView && !Tools.viewToolActive && GetPlanePointFromMouse(out var point))
                    {
                        lastMouseMove = point;
                        Vector3 pos = selection.transform.position;
                        pos.x = point.x;
                        pos.y = point.y;
                        selection.transform.position = pos;
                        EditorUtility.SetDirty(selection);

                        e.Use();
                    }
                    break;
                }
            case EventType.MouseDrag:
                {
                    if (isSceneView && !Tools.viewToolActive && GetPlanePointFromMouse(out var point) && (point - lastMouseMove).magnitude >= 0.5f)
                    {
                        if (dragging)
                        {
                            var radius = (point - lastMouseMove).normalized;
                            var angle1 = Mathf.Atan2(origRadius.y, origRadius.x);
                            var angle2 = Mathf.Atan2(radius.y, radius.x);

                            Quaternion quat = origQuat;
                            var euler = quat.eulerAngles;
                            euler.z += (angle2 - angle1) * Mathf.Rad2Deg;
                            quat.eulerAngles = euler;
                            selection.transform.localRotation = quat;
                            EditorUtility.SetDirty(selection);
                        }
                        else
                        {
                            dragging = true;
                            origRadius = (point - lastMouseMove).normalized;
                            origQuat = selection.transform.localRotation;
                        }

                        e.Use();
                    }
                    break;
                }
            case EventType.MouseUp:
                {
                    if (isSceneView && !Tools.viewToolActive && !dragging)
                    {
                        UpdateSelection(false);
                        e.Use();
                    }

                    break;
                }
            default:
                break;
        }
    }

    private static bool GetPlanePoint(Ray ray, out Vector2 point)
    {
        var plane = new Plane(Vector3.forward, Vector3.zero);
        if (plane.Raycast(ray, out var distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        var ray2 = new Ray(ray.origin, -ray.direction);
        if (plane.Raycast(ray2, out distance))
        {
            point = ray2.GetPoint(distance);
            return true;
        }

        point = Vector2.zero;
        return false;
    }

    private static bool GetPlanePointFromMouse(out Vector2 point) => GetPlanePoint(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out point);

    private static string UniqueName(string name, Transform parent)
    {
        var set = new HashSet<string>();
        foreach (Transform t in parent) if (t.name.StartsWith(name)) set.Add(t.name);

        string baseName = name;
        int i = 0;
        while (set.Contains(name)) name = $"{baseName} ({++i})";
        return name;
    }

    private static MethodInfo getDefaultParentObjectIfSet = typeof(SceneView).GetMethod("GetDefaultParentObjectIfSet", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
    private static Transform GetDefaultParent()
    {
        Transform t = getDefaultParentObjectIfSet.Invoke(null, new object[] { }) as Transform;
        return t != null ? t : GameObject.Find("_Scenery").transform;
    }

    private void UpdateSelection(bool destroy)
    {
        Vector3 position = Vector3.zero;
        Quaternion rotation = Quaternion.identity;
        Vector3 scale = Vector3.one;
        if (selection != null)
        {
            position = selection.transform.position;
            rotation = selection.transform.localRotation;
            scale = selection.transform.localScale;

            if (destroy) DestroyImmediate(selection);
            else history.Push(selection);
        }

        var template = brush.Groups[group].Instances[instance];
        previousGroup = group;
        previousInstance = instance;

        var parent = GetDefaultParent();
        if (PrefabUtility.IsAnyPrefabInstanceRoot(template))
        {
            selection = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(template), parent);
            PrefabUtility.SetPropertyModifications(selection, PrefabUtility.GetPropertyModifications(template));
        }
        else selection = Instantiate(template, parent);

        selection.name = UniqueName(template.name, parent);

        position.z = template.transform.position.z;
        selection.transform.position = position;
        selection.transform.rotation = rotation;
        selection.transform.localScale = scale;
        EditorUtility.SetDirty(selection);
    }

    public override void OnWillBeDeactivated()
    {
        if (selection != null) DestroyImmediate(selection);

        base.OnWillBeDeactivated();
    }

    private static System.Type previousToolType;

    private static void Deactivate()
    {
        if (previousToolType != null) ToolManager.SetActiveTool(previousToolType);
        else ToolManager.RestorePreviousPersistentTool();
    }

    [Shortcut("Activate Asset Brush", KeyCode.A)]
    public static void ActivateTool()
    {
        if (ToolManager.activeToolType == typeof(AssetBrushTool))
        {
            Deactivate();
            return;
        }

        var filtered = Selection.gameObjects.Select(go => go.GetComponent<AssetBrush>()).Where(ab => ab != null).ToList();
        if (filtered.Count == 1)
        {
            previousBrush = filtered[0];
            previousGroup = 0;
            previousInstance = 0;

            DoActivateAssetBrush();
        }
        else if (previousBrush != null && previousBrush.gameObject != null) DoActivateAssetBrush();
        else Debug.Log("Couldn't start AssetBrushTool");
    }

    private static void DoActivateAssetBrush()
    {
        previousToolType = ToolManager.activeToolType;
        ToolManager.SetActiveTool<AssetBrushTool>();
    }
}
