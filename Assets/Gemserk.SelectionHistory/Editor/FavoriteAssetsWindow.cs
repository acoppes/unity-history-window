using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEditor.UIElements;
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
            var titleContent = EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteWindowIconName);
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

        private static bool CanBeFavorite(Object reference)
        {
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(reference)))
            {
                return true;
            }
            return false;
        }

        private static void FavoriteElements(Object[] references)
        {
            var favorites = FavoritesAsset.instance;

            foreach (var reference in references)
            {
                if (favorites.IsFavorite(reference))
                    continue;
            
                if (CanBeFavorite(reference))
                {
                    favorites.AddFavorite(new FavoritesAsset.Favorite
                    {
                        reference = reference
                    });   
                }
            }
        }

        private FavoritesAsset _favorites;

        private StyleSheet styleSheet;

        private VisualTreeAsset favoriteElementTreeAsset;

        private ToolbarSearchField searchToolbar;
        private VisualElement favoritesParent;
        
        private string searchText;
        
        private void GetDefaultElements()
        {
            if (styleSheet == null)
            {
                styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                    .OfType<StyleSheet>().FirstOrDefault();
            }
            
            if (favoriteElementTreeAsset == null)
            {
                favoriteElementTreeAsset = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "FavoriteElement")
                    .OfType<VisualTreeAsset>().FirstOrDefault();
            }
        }
        
        private void OnDisable()
        {
            if (_favorites != null)
            {
                _favorites.OnFavoritesUpdated -= OnFavoritesUpdated;
            }
            
            styleSheet = null;
            favoriteElementTreeAsset = null;
        }

        public void OnEnable()
        {
            GetDefaultElements();
            
            _favorites = FavoritesAsset.instance;
            _favorites.OnFavoritesUpdated += OnFavoritesUpdated;
            
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);

            root.Add(CreateSearchToolbar());

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
        }

        private void OnFavoritesUpdated(FavoritesAsset favorites)
        {
            // var root = rootVisualElement;
            // root.Clear();
            ReloadRoot();
        }

        private VisualElement CreateSearchToolbar()
        {
            searchToolbar = new ToolbarSearchField();
            searchToolbar.AddToClassList("searchToolbar");
            searchToolbar.RegisterValueChangedCallback(evt =>
            {
                searchText = evt.newValue;
                ReloadRoot();
            });

            return searchToolbar;
        }
        
        private void ReloadRoot()
        {
            var root = rootVisualElement;

            // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FavoriteElement.uxml");
            if (favoritesParent == null)
            {
                favoritesParent = new ScrollView(ScrollViewMode.Vertical);
                root.Add(favoritesParent);
            }
            else
            {
                favoritesParent.Clear();
            }
            
            string[] searchTexts = null;
            if (!string.IsNullOrEmpty(searchText))
            {
                searchText = searchText.TrimStart().TrimEnd();
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchTexts = searchText.Split(' ');
                }
            }

            for (var i = 0; i < _favorites.favoritesList.Count; i++)
            {
                var assetReference = _favorites.favoritesList[i].reference;

                if (assetReference == null)
                    continue;

                var testName = assetReference.name.ToLower();
                    
                if (searchTexts != null && searchTexts.Length > 0)
                {
                    var match = true;
                        
                    foreach (var text in searchTexts)
                    {
                        if (!testName.Contains(text.ToLower()))
                        {
                            match = false;
                        }
                    }

                    if (!match)
                    {
                        continue;
                    }
                }
                
                var elementTree = favoriteElementTreeAsset.CloneTree();
                var favoriteRoot = elementTree.Q<VisualElement>("Root");
                
                var dragArea = elementTree.Q<VisualElement>("DragArea");
                
                var isSceneAsset = assetReference is SceneAsset;
                var isAsset = !isSceneAsset;

                if (dragArea != null)
                {
                    dragArea.AddManipulator(new FavoriteElementDragManipulator(assetReference));
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
                    removeIcon.tooltip = "Remove";
                    
                    removeIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        FavoritesAsset.instance.RemoveFavorite(assetReference);
                    });
                }
                
                var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
                if (openPrefabIcon != null)
                {
                    // removeIcon.image = AssetPreview.GetMiniThumbnail(assetReference);
                    openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openAssetIconName).image;
                    openPrefabIcon.tooltip = "Open";

                    openPrefabIcon.RemoveFromClassList("hidden");

                    openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        AssetDatabase.OpenAsset(assetReference);
                    });
                }
                
                var label = elementTree.Q<Label>("Favorite");
                if (label != null)
                {
                    label.text = assetReference.name;
                }

                favoritesParent.Add(favoriteRoot);
            }

            var receiveDragArea = new VisualElement();
            receiveDragArea.style.flexGrow = 1;
            root.Add(receiveDragArea);
        }
    }
}