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
    private int xSign = 1;
    private int scalePower = 0;

    private readonly Stack<GameObject> history = new Stack<GameObject>();
    private GameObject selection;

    public override GUIContent toolbarIcon => base.toolbarIcon;

    public override bool IsAvailable() => true;

    private void OnHierarchyGUI(int instanceId, Rect selectionRect)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));

        var e = Event.current;
        if (e.type == EventType.KeyDown && HandleKeyEvent(e.keyCode)) e.Use();
    }

    public override void OnActivated()
    {
        brush = previousBrush;
        group = previousGroup;
        instance = previousInstance;
        xSign = 1;
        scalePower = 0;

        UpdateSelection(true);
        LogGroup();

        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
    }

    private Vector2 lastMouseMove;
    private bool dragging = false;
    private Quaternion origQuat;
    private Vector2 origRadius;
    private Vector3 origScale;

    private void UpdateScale()
    {
        Vector3 scale = origScale;
        scale.x *= xSign;
        var p = Mathf.Pow(1.05f, scalePower);
        scale.x *= p;
        scale.y *= p;
        selection.transform.localScale = scale;
    }

    private void LogGroup() => Debug.Log($"Group: {brush.Groups[group].name}");

    private bool HandleKeyEvent(KeyCode code)
    {
        if (code == KeyCode.Space) UpdateSelection(false);
        else if (code == KeyCode.DownArrow)
        {
            scalePower--;
            UpdateScale();
        }
        else if (code == KeyCode.UpArrow)
        {
            scalePower++;
            UpdateScale();
        }
        else if (code == KeyCode.W)
        {
            if (++group == brush.Groups.Count) group = 0;
            UpdateSelection(true);
            LogGroup();
        }
        else if (code == KeyCode.S)
        {
            if (--group == -1) group = brush.Groups.Count - 1;
            UpdateSelection(true);
            LogGroup();
        }
        else if (code == KeyCode.Q)
        {
            if (--instance == -1) instance = brush.Groups[group].Instances.Count - 1;
            UpdateSelection(true);
        }
        else if (code == KeyCode.E)
        {
            if (++instance == brush.Groups[group].Instances.Count) instance = 0;
            UpdateSelection(true);
        }
        else if (code == KeyCode.F)
        {
            xSign *= -1;
            UpdateScale();
        }
        else if (code == KeyCode.R)
        {
            var quat = selection.transform.localRotation;
            var euler = quat.eulerAngles;
            euler.z += 90;
            quat.eulerAngles = euler;
            selection.transform.localRotation = quat;
        }
        else if (code == KeyCode.Z)
        {
            if (history.Count > 0) DestroyImmediate(history.Pop());
        }
        else if (code == KeyCode.X)
        {
            scalePower = 0;
            xSign = 1;
            UpdateScale();
            selection.transform.localRotation = Quaternion.identity;
        }
        else if (code == KeyCode.Escape)
        {
            while (history.Count > 0) DestroyImmediate(history.Pop());
            Deactivate();
        }
        else return false;

        return true;
    }

    public override void OnToolGUI(EditorWindow window)
    {
        bool isSceneView = window is SceneView;
        Vector2 mousePoint = Vector3.zero;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Keyboard));

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                {
                    if (HandleKeyEvent(e.keyCode)) e.Use();
                    break;
                }
            case EventType.MouseMove:
                {
                    dragging = false;
                    if (isSceneView && !Tools.viewToolActive && GetPlanePointFromMouse(out mousePoint))
                    {
                        lastMouseMove = mousePoint;
                        Vector3 pos = selection.transform.position;
                        pos.x = mousePoint.x;
                        pos.y = mousePoint.y;
                        selection.transform.position = pos;
                        EditorUtility.SetDirty(selection);

                        e.Use();
                    }
                    break;
                }
            case EventType.MouseDrag:
                {
                    if (isSceneView && !Tools.viewToolActive && GetPlanePointFromMouse(out mousePoint) && (mousePoint - lastMouseMove).magnitude >= 0.5f)
                    {
                        if (dragging)
                        {
                            var radius = (mousePoint - lastMouseMove).normalized;
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
                            origRadius = (mousePoint - lastMouseMove).normalized;
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
        origScale = template.transform.localScale;

        selection.name = UniqueName(template.name, parent);

        position.z = template.transform.position.z;
        selection.transform.position = position;
        selection.transform.rotation = rotation;
        UpdateScale();

        Selection.activeTransform = selection.transform;
        EditorUtility.SetDirty(selection);
    }

    public override void OnWillBeDeactivated()
    {
        EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;

        if (selection != null) DestroyImmediate(selection);
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
