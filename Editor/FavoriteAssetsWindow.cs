using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        private VisualTreeAsset searchToolbarViewTree;
        private VisualTreeAsset favoriteElementTreeAsset;

        public VisualElement favoritesParent;
        
        private string searchText;
        
        private void OnDisable()
        {
            if (_favorites != null)
            {
                _favorites.OnFavoritesUpdated -= OnFavoritesUpdated;
            }
        }

        public void OnEnable()
        {
            styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                .OfType<StyleSheet>().FirstOrDefault();
            searchToolbarViewTree = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "SearchToolbar")
                .OfType<VisualTreeAsset>().FirstOrDefault();
            favoriteElementTreeAsset = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "FavoriteElement")
                .OfType<VisualTreeAsset>().FirstOrDefault();
            
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
            var elementTree = searchToolbarViewTree.CloneTree();
            var searchToolbarTree = elementTree.Q<VisualElement>("SearchToolbar");
            
            var textField = elementTree.Q<TextField>("Search");
            textField.RegisterValueChangedCallback(delegate(ChangeEvent<string> change)
            {
                // set current view elements filter
                // Debug.Log("new filter " + change.newValue);
                searchText = change.newValue;
                
                ReloadRoot();
            });

            var icon = elementTree.Q<Image>("Icon");
            if (icon != null)
            {
                icon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.searchIconName).image;
            }
            
            var clearIcon = elementTree.Q<Image>("Clear");
            if (clearIcon != null)
            {
                clearIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.clearSearchToolbarIconName).image;
                clearIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    textField.value = "";
                });
            }
            
            return searchToolbarTree;
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
                
                var isPrefabAsset = PrefabUtility.IsPartOfPrefabAsset(assetReference);
                var isSceneAsset = assetReference is SceneAsset;

                if (dragArea != null)
                {
                    dragArea.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        if (evt.button == 0)
                        {
                            Selection.activeObject = assetReference;
                        }
                        else
                        {
                            EditorGUIUtility.PingObject(assetReference);
                        }
                        
                        dragArea.userData = null;
                    });
                    dragArea.RegisterCallback<MouseDownEvent>(evt =>
                    {
                        if (evt.button == 0)
                        {
                            DragAndDrop.PrepareStartDrag();

                            var objectReferences = new[] {assetReference};
                            DragAndDrop.paths = new[]
                            {
                                AssetDatabase.GetAssetPath(assetReference)
                            };

                            DragAndDrop.objectReferences = objectReferences;
                            DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(assetReference));
                        }
                    });

                    dragArea.RegisterCallback<DragUpdatedEvent>(evt =>
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    });
                    
                    dragArea.RegisterCallback<PointerDownEvent>(evt =>
                    {
                        if (evt.button == 0 && evt.clickCount == 2)
                        {
                            // Debug.Log("DOUBLE CLICK");
                            if (isPrefabAsset)
                            {
                                AssetDatabase.OpenAsset(assetReference);
                            }

                            if (isSceneAsset)
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(assetReference));
                                }
                            }
                        }
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
                        FavoritesAsset.instance.RemoveFavorite(assetReference);
                    });
                }
                
                var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
                if (openPrefabIcon != null)
                {
                    // removeIcon.image = AssetPreview.GetMiniThumbnail(assetReference);
                    openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openPrefabIconName).image;

                    if (isPrefabAsset || isSceneAsset)
                    {
                        openPrefabIcon.RemoveFromClassList("hidden");
                    }
                    
                    openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        if (isPrefabAsset)
                        {
                            AssetDatabase.OpenAsset(assetReference);
                        } else if (isSceneAsset)
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(assetReference));
                            }
                            
                        }
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