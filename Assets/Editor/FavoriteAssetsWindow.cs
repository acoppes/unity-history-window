using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class FavoriteAssetsWindow : EditorWindow
{
    [MenuItem("Window/UIElements/FavoriteAssetsWindow")]
    public static void ShowExample()
    {
        var wnd = GetWindow<FavoriteAssetsWindow>();
        wnd.titleContent = new GUIContent("FavoriteAssetsWindow");
    }

    public void OnEnable()
    {
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/FavoriteAssetsWindow.uss");

        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;
        root.styleSheets.Add(styleSheet);

        // // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);
        //
        // // Import UXML
        
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FavoriteAssetsWindow.uxml");
        
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        root.Add(scroll);
        
        for (var i = 0; i < 50; i++)
        {
            var tree = visualTree.CloneTree();
            var favoriteElement = tree.Q<ObjectField>("Favorite");
            favoriteElement.allowSceneObjects = false;
            favoriteElement.objectType = typeof(SceneAsset);
            scroll.Add(favoriteElement);
        }

        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
    }
}