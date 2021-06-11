using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    public class FavoriteAssetsWindow : EditorWindow
    {
        [MenuItem("Window/Gemserk/Favorites")]
        public static void OpenWindow()
        {
            var window = GetWindow<FavoriteAssetsWindow>();
            var titleContent = EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteIconName);
            titleContent.text = "Favorites";
            titleContent.tooltip = "Favorite assets window";
            window.titleContent = titleContent;
        }

        [MenuItem("Assets/Favorite Item")]
        [Shortcut("Gemserk/Favorite Item", null, KeyCode.F, ShortcutModifiers.Shift | ShortcutModifiers.Alt)]
        public static void Favorite()
        { 
            FavoriteElements(Selection.objects);
        }

        private static void FavoriteElements(Object[] references)
        {
            var favorites = FavoritesController.Favorites;

            foreach (var reference in references)
            {
                if (favorites.IsFavorite(reference))
                    continue;
            
                if (favorites.CanBeFavorite(Selection.activeObject))
                {
                    favorites.AddFavorite(new Favorites.Favorite
                    {
                        reference = reference
                    });   
                }
            }
        }

        private Favorites _favorites;

        public StyleSheet styleSheet;

        public VisualTreeAsset favoriteElementTreeAsset;
        
        private void OnDisable()
        {
            if (_favorites != null)
            {
                _favorites.OnFavoritesUpdated -= OnFavoritesUpdated;
            }
        }

        public void OnEnable()
        {
            _favorites = FavoritesController.Favorites;
            _favorites.OnFavoritesUpdated += OnFavoritesUpdated;
            
            // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/FavoriteAssetsWindow.uss");

            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);

            // // VisualElements objects can contain other VisualElement following a tree hierarchy.
            // VisualElement label = new Label("Hello World! From C#");
            // root.Add(label);
            //
            // // Import UXML
            
            root.RegisterCallback<DragPerformEvent>(evt =>
            {
                DragAndDrop.AcceptDrag();
                FavoriteElements(DragAndDrop.objectReferences);
            });
            
            root.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            });
            

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

            // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FavoriteElement.uxml");

            var scroll = new ScrollView(ScrollViewMode.Vertical);
            root.Add(scroll);

            for (var i = 0; i < _favorites.favoritesList.Count; i++)
            {
                var assetReference = _favorites.favoritesList[i].reference;

                if (assetReference == null)
                    continue;
                
                var elementTree = favoriteElementTreeAsset.CloneTree();

                var ping = elementTree.Q<VisualElement>("Ping");
                if (ping != null)
                {
                    ping.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        EditorGUIUtility.PingObject(assetReference);
                    });
                }
                
                var icon = elementTree.Q<Image>("Icon");
                if (icon != null)
                {
                    icon.image = AssetPreview.GetMiniThumbnail(assetReference);
                }
                
                var removeIcon = elementTree.Q<Image>("RemoveIcon");
                if (removeIcon != null)
                {
                    // removeIcon.image = AssetPreview.GetMiniThumbnail(assetReference);
                    removeIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.removeIconName).image;
                    
                    removeIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        FavoritesController.Favorites.RemoveFavorite(assetReference);
                    });
                }
                
                var label = elementTree.Q<Label>("Favorite");
                if (label != null)
                {
                    label.text = assetReference.name;
                }

                scroll.Add(elementTree);
            }

            var dragArea = new VisualElement();
            dragArea.style.flexGrow = 1;
            root.Add(dragArea);
        }
    }
}