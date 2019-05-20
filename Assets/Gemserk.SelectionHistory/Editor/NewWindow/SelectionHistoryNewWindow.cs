using System;
using Gemserk;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

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

    private VisualElement _historyObjectsContainer;

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

        _historyObjectsContainer = new VisualElement();
        
        root.Add(_historyObjectsContainer);

        _styleSheet = LoadStyleSheet();
        _visualTreeAsset = LoadTreeAsset();

        if (_styleSheet == null || _visualTreeAsset == null)
        {
            Debug.LogError("Failed to initialize selection history");
            return;
        }
        
        root.styleSheets.Add(_styleSheet);
        
        var clearButton = new Button(delegate
        {
            selectionHistory.Clear();
            // _historyObjectsContainer.Clear();
            // clear list too
        });
        clearButton.text = "Clear";
        
        root.Add(clearButton);
        
        var scheduledAction = root.schedule.Execute(() =>
        {
            // textFields.ForEach(t => t.value = m_Tank.tankName);
            // integerFields.ForEach(t => t.value = m_Tank.tankSize);
        });
        scheduledAction.Every(100); // ms

        selectionHistory.objectAdded += AddSelectionField;
        selectionHistory.cleared += () =>
        {
            _historyObjectsContainer.Clear();
        };

        // Selection.selectionChanged += OnSelectionChanged;
        
        // regenerate
//        _historyObjectsContainer.Clear();
        
        selectionHistory.History.ForEach(AddSelectionField);
    }

    public void OnDisable()
    {
        selectionHistory.objectAdded -= AddSelectionField;
        // Selection.selectionChanged -= OnSelectionChanged;
    }

    private void AddSelectionField(Object objectAdded)
    {
        var tree = _visualTreeAsset.CloneTree();
        var selectionContainer = tree.Q("Selection");
        var objectField = selectionContainer.Q<ObjectField>( );

        var button = selectionContainer.Q<Button>();
        button.text = "Ping";
        
        // selectionHistory.
        objectField.SetValueWithoutNotify(objectAdded);
        
        _historyObjectsContainer.Add(selectionContainer);
    }
}