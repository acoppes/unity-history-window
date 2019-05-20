using Gemserk;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class SelectionHistoryNewWindow : EditorWindow
{
    private static readonly string StyleSheetFileName = "SelectionHistoryNewWindow";
    private static readonly string VisualTreeFileName = "SelectionHistoryNewWindow";
    
    private static Vector2 _windowMinSize = new Vector2(300, 200);

    [Shortcut("Selection History/Show", null, KeyCode.H, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
    [MenuItem("Window/Gemserk/New Selection History")]
    public static void OpenSelectionHistoryWindow()
    {
        var window = GetWindow<SelectionHistoryNewWindow>();
        window.titleContent = new GUIContent("SelectionHistoryNewWindow");
        window.minSize = _windowMinSize;
    }
    
    private static SelectionHistory selectionHistory
    {
        get { return SelectionHistoryContext.SelectionHistory; }
    }

    private StyleSheet _styleSheet;

    private VisualTreeAsset _visualTreeAsset;

    private static StyleSheet LoadStyleSheet()
    {
        var guids = AssetDatabase.FindAssets("t:StyleSheet " + StyleSheetFileName);

        if (guids.Length == 0)
            return null;
        
        return AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }

    private static VisualTreeAsset LoadTreeAsset()
    {
        var guids = AssetDatabase.FindAssets("t:VisualTreeAsset " + VisualTreeFileName);

        if (guids.Length == 0)
            return null;

        return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
    }
    
    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        _styleSheet = LoadStyleSheet();
        _visualTreeAsset = LoadTreeAsset();

        if (_styleSheet == null || _visualTreeAsset == null)
        {
            Debug.LogError("Failed to initialize selection history");
            return;
        }
        
        var clearButton = new Button(delegate
        {
            selectionHistory.Clear();
        });
        clearButton.text = "Clear";
        
        root.Add(clearButton);
    }
}