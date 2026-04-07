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

        [SerializeField]
        private VisualTreeAsset windowTreeAsset = default;

        // [SerializeField]
        // private VisualTreeAsset windowTreeAsset = default;

        private FavoritesAsset _favorites;
        
        [SerializeField]
        private StyleSheet styleSheet;

        [SerializeField]
        private VisualTreeAsset favoriteElementTreeAsset;

        private ToolbarSearchField searchToolbar;
        private ListView favoritesListView;
        
        private string[] searchTexts = null;
        
        private void GetDefaultElements()
        {
            if (!styleSheet)
            {
                styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                    .OfType<StyleSheet>().FirstOrDefault();
            }
            
            if (!favoriteElementTreeAsset)
            {
                favoriteElementTreeAsset = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "FavoriteElement")
                    .OfType<VisualTreeAsset>().FirstOrDefault();
            }
            
            if (!windowTreeAsset)
            {
                windowTreeAsset = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "FavoritesWindow")
                    .OfType<VisualTreeAsset>().FirstOrDefault();
            }
        }
        
        private void OnDisable()
        {
            if (_favorites)
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
            // ReloadRoot();
            favoritesListView?.RefreshItems();
        }

        private VisualElement CreateSearchToolbar()
        {
            searchToolbar = new ToolbarSearchField();
            searchToolbar.AddToClassList("searchToolbar");
            searchToolbar.RegisterValueChangedCallback(evt =>
            {
                var searchText = evt.newValue;
                
                if (!string.IsNullOrEmpty(searchText))
                {
                    searchText = searchText.TrimStart().TrimEnd();
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        searchTexts = searchText.Split(' ');
                    }
                }
                else
                {
                    searchTexts = null;
                }
                
                favoritesListView?.RefreshItems();
            });

            return searchToolbar;
        }
        
        private void ReloadRoot()
        {
            var root = rootVisualElement;

            // var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/FavoriteElement.uxml");
            if (favoritesListView == null)
            {
                var elementTree = windowTreeAsset.CloneTree();
                favoritesListView = elementTree.Q<ListView>("FavoritesList");
                
                favoritesListView.itemsSource = _favorites.favoritesList;
                favoritesListView.bindItem = BindFavorite;
                favoritesListView.makeItem = MakeFavoritesElement;
#if UNITY_2021_1_OR_NEWER
                favoritesListView.itemIndexChanged += OnReordered;
#endif
                root.Add(favoritesListView);
            }
           
            var receiveDragArea = new VisualElement();
            receiveDragArea.style.flexGrow = 1;
            root.Add(receiveDragArea);
        }

        private void OnReordered(int a, int b)
        {
            _favorites.OnFavoritesModified();
        }

        private VisualElement MakeFavoritesElement()
        {
            var elementTree = favoriteElementTreeAsset.CloneTree();
            var favoriteRoot = elementTree.Q<VisualElement>("Root");
            
            var dragArea = elementTree.Q<VisualElement>("DragArea");
            dragArea.AddManipulator(new FavoriteElementDragManipulator());
            
            var removeIcon = elementTree.Q<Image>("RemoveIcon");
            if (removeIcon != null)
            {
                removeIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.removeIconName).image;
                removeIcon.tooltip = "Remove";
                removeIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var assetReference = removeIcon.userData as Object;
                    FavoritesAsset.instance.RemoveFavorite(assetReference);
                });
            }

            var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
            if (openPrefabIcon != null)
            {
                openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openAssetIconName).image;
                openPrefabIcon.tooltip = "Open";
                openPrefabIcon.RemoveFromClassList("hidden");
                openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var assetReference = openPrefabIcon.userData as Object;
                    AssetDatabase.OpenAsset(assetReference);
                });
            }
            
            return favoriteRoot;
        }

        private void BindFavorite(VisualElement visualElement, int elementIndex)
        {
            var favorite = _favorites.favoritesList[elementIndex];
            var assetReference = favorite.reference;
            
            // this is the reorderable item, hiding that to not show the handle with empty item.
            visualElement.parent.parent.style.display = DisplayStyle.Flex;
            
            var label = visualElement.Q<Label>("Favorite");
            
            if (!assetReference)
            {
                label.AddToClassList("favorites-missing-reference");
                label.text = "(missing reference)";
                if (!string.IsNullOrEmpty(favorite.assetPath))
                {
                    label.text = $"{favorite.assetPath}";
                }
                return;
            }
            else
            {
                label.RemoveFromClassList("favorites-missing-reference");
            }
            
            var assetName = assetReference.name;
            
            if (string.IsNullOrEmpty(assetName))
            {
                assetName = assetReference.GetType().Name;
            }
            
            var testName = assetName.ToLower();
            
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
                    visualElement.parent.parent.style.display = DisplayStyle.None;
                }
            }
            
            var dragArea = visualElement.Q<VisualElement>("DragArea");
            
            // var isSceneAsset = assetReference is SceneAsset;
            // var isAsset = !isSceneAsset;

            if (dragArea != null)
            {
                dragArea.userData = assetReference;
            }
            
            var icon = visualElement.Q<Image>("Icon");
            if (icon != null)
            {
                icon.image = AssetPreview.GetMiniThumbnail(assetReference);
            }
            
            var removeIcon = visualElement.Q<Image>("RemoveIcon");
            removeIcon.userData = assetReference;
      
            var openPrefabIcon = visualElement.Q<Image>("OpenPrefabIcon");
            openPrefabIcon.userData = assetReference;
            
            label.text = assetName;
        }
    }
}