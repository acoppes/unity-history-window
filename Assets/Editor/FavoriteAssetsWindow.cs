using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[InitializeOnLoad]
public static class FavoriteAssetsWindowInitialization
{
    static FavoriteAssetsWindowInitialization()
    {
        FavoriteAssetsWindow.RegisterSelectionListener();
    }
}

public class FavoriteAssetsWindow : EditorWindow
{
    [MenuItem("Window/UIElements/FavoriteAssetsWindow")]
    public static void ShowExample()
    {
        var wnd = GetWindow<FavoriteAssetsWindow>();
        wnd.titleContent = new GUIContent("FavoriteAssetsWindow");
    }
    
    public static void RegisterSelectionListener()
    {
        Selection.selectionChanged += SelectionRecorder;
    }
    
    private static void SelectionRecorder ()
    {
        if (Selection.activeObject != null) {
            // just for testing...
            
            if (Selection.activeObject is GameObject go)
            {
                if (go.scene != null)
                    return;
            }
            
            var favoritesController = FavoritesController.Instance;
            favoritesController.AddFavorite(new Favorites.Favorite
            {
                reference = Selection.activeObject
            });
        } 
    }

    private FavoritesController _favoritesController;

    public StyleSheet styleSheet;

    private void OnDisable()
    {
        if (_favoritesController != null)
        {
            _favoritesController.OnFavoritesUpdated -= OnFavoritesUpdated;
        }
    }

    public void OnEnable()
    {
        _favoritesController = FavoritesController.Instance;
        _favoritesController.OnFavoritesUpdated += OnFavoritesUpdated;
            
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/FavoriteAssetsWindow.uss");

        // Each editor window contains a root VisualElement object
        var root = rootVisualElement;
        root.styleSheets.Add(styleSheet);

        // // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);
        //
        // // Import UXML

        ReloadRoot();
        // A stylesheet can be added to a VisualElement.
        // The style will be applied to the VisualElement and all of its children.
    }

    private void OnFavoritesUpdated(Favorites favorites)
    {
        var root = rootVisualElement;
        root.Clear();
        
        ReloadRoot();
    }

    private void ReloadRoot()
    {
        var root = rootVisualElement;
        
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FavoriteElement.uxml");
        
        var scroll = new ScrollView(ScrollViewMode.Vertical);
        root.Add(scroll);

        // var guids = AssetDatabase.FindAssets("t:Scene");
        // var sceneAsset = 
        //     guids.Select(g => AssetDatabase.LoadAssetAtPath<SceneAsset>(AssetDatabase.GUIDToAssetPath(g))).First();

        var favorites = _favoritesController.GetFavorites();
        
        for (var i = 0; i < favorites.favoritesList.Count; i++)
        {
            var assetReference = favorites.favoritesList[i].reference;
            var tree = visualTree.CloneTree();
            
            var icon = tree.Q<Image>("Icon");
            icon.image = AssetPreview.GetMiniThumbnail(assetReference);
            
            var favoriteElement = tree.Q<Button>("Favorite");
            favoriteElement.text = assetReference.name;
            favoriteElement.clicked += delegate
            {
                EditorGUIUtility.PingObject(assetReference);
            };
            scroll.Add(tree);
        }

    }
}