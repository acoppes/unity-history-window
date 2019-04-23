using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class SelectionHistoryNewWindow : EditorWindow
{
    [MenuItem("Window/UIElements/SelectionHistoryNewWindow")]
    public static void ShowExample()
    {
        var window = GetWindow<SelectionHistoryNewWindow>();
        window.titleContent = new GUIContent("SelectionHistoryNewWindow");
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        VisualElement label = new Label("Hello World! From C#");
        root.Add(label);

        // Import UXML
      //  var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Gemserk.SelectionHistory/Editor/SelectionHistoryNewWindow.uxml");
       // VisualElement labelFromUXML = visualTree.CloneTree();
       // root.Add(labelFromUXML);

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Gemserk.SelectionHistory/Editor/SelectionHistoryNewWindow.uss");
        VisualElement labelWithStyle = new Label("Hello World! With Style");
        labelWithStyle.styleSheets.Add(styleSheet);
        root.Add(labelWithStyle);
    }
}