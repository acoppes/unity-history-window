using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class SelectionHistoryNewWindow : EditorWindow
{
    private static readonly string StyleSheetFileName = "SelectionHistoryNewWindow";
    
    private static Vector2 _windowMinSize = new Vector2(300, 200);

    [MenuItem("Window/Gemserk/New Selection History")]
    public static void ShowExample()
    {
        var window = GetWindow<SelectionHistoryNewWindow>();
        window.titleContent = new GUIContent("SelectionHistoryNewWindow");
        window.minSize = _windowMinSize;
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        var styleSheetGuids = AssetDatabase.FindAssets("t:StyleSheet " + StyleSheetFileName);

        if (styleSheetGuids.Length == 0)
        {
            Debug.LogError("Failed to find selection history style sheet");
            return;
        }

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(styleSheetGuids[0]));
        // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Gemserk.SelectionHistory/Editor/SelectionHistoryNewWindow.uss");
        VisualElement labelWithStyle = new Label("Hello World! With Style");
        labelWithStyle.styleSheets.Add(styleSheet);
        root.Add(labelWithStyle);
    }
}