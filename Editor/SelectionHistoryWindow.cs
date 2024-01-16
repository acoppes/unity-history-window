using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Gemserk
{
    public static class SelectionHistoryWindowExtensions
    {
        public static SelectionHistory.Entry GetEntry(this SelectionHistory selectionHistory, int index)
        {
            if (index < 0 || index >= selectionHistory.History.Count)
            {
                return null;
            }

            return selectionHistory.History[index];
        }

        public static bool IsSceneAsset(this SelectionHistory.Entry entry)
        {
            return entry.isReferenced && entry.isAsset && entry.reference is SceneAsset;
        }

        public static bool IsPrefabAsset(this SelectionHistory.Entry entry)
        {
            return entry.isReferenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
        }
    }
    
    public class SelectionHistoryWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/Gemserk/Selection History %#h")]
        public static void OpenWindow()
        {
            var window = GetWindow<SelectionHistoryWindow>();
            var titleContent = EditorGUIUtility.IconContent("Refresh");
            titleContent.text = "History";
            titleContent.tooltip = "Objects selection history";
            window.titleContent = titleContent;
        }
        
        public StyleSheet styleSheet;

        public VisualTreeAsset historyElementViewTree;

        private SelectionHistory selectionHistory;

        private ScrollView mainScrollElement;
        private List<VisualElement> visualElements = new List<VisualElement>();

        private Button removeUnloadedButton;
        private Button removeDestroyedButton;

        private void OnDisable()
        {
            //EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded -= OnHistoryEntryAdded;
            }
            
            Selection.selectionChanged -= OnSelectionChanged;
        }

        public void OnEnable()
        {
            //EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            selectionHistory = SelectionHistoryReference.SelectionHistory;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded += OnHistoryEntryAdded;
            }

            FavoritesController.Favorites.OnFavoritesUpdated += delegate
            {
                ReloadRootAndRemoveUnloadedAndDuplicated();
            };
            
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);
            
            RegenerateUI();
            
            ReloadRootAndRemoveUnloadedAndDuplicated();
            
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void RegenerateUI()
        {
            var root = rootVisualElement;
            root.Clear();
            
            visualElements.Clear();
            
            mainScrollElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "MainScroll"
            };
            
            root.Add(mainScrollElement);
            
            CreateMaxElements(selectionHistory, mainScrollElement);
            
            var clearButton = new Button(delegate
            {
                selectionHistory.Clear();
                ReloadRoot();
            }) {text = "Clear"};
            
            root.Add(clearButton);
            
            // // this is just for development
            // var refreshButton = new Button(delegate
            // {
            //     ReloadRoot();
            // }) {text = "Refresh (dev)"};
            //
            // root.Add(refreshButton);
            
            removeUnloadedButton = new Button(delegate
            {
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);
                // ReloadRootAndRemoveUnloadedAndDuplicated();
                ReloadRoot();
            }) {text = "Remove Unloaded"};
            root.Add(removeUnloadedButton);
            
            removeDestroyedButton = new Button(delegate
            {
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
                // ReloadRootAndRemoveUnloadedAndDuplicated();
                ReloadRoot();
            }) {text = "Remove destroyed"};
            root.Add(removeDestroyedButton);
        }

        private void CreateMaxElements(SelectionHistory selectionHistory, VisualElement parent)
        {
            var size = selectionHistory.historySize;

            for (int i = 0; i < size; i++)
            {
                var elementTree = CreateHistoryVisualElement(i);
                parent.Add(elementTree);
                
                visualElements.Add(elementTree);
            }
        }

        private VisualElement CreateHistoryVisualElement(int index)
        {
            var elementTree = historyElementViewTree.CloneTree();
            var historyIndex = index;
            
            var dragArea = elementTree.Q<VisualElement>("DragArea");
            if (dragArea != null)
            {
                dragArea.RegisterCallback<MouseUpEvent>(evt =>
                {
                    var entry = selectionHistory.GetEntry(historyIndex);
                    if (entry == null || !entry.isReferenced)
                    {
                        return;
                    }
                    
                    if (evt.button == 0)
                    {
                        selectionHistory.SetSelection(entry.Reference);
                        Selection.activeObject = entry.Reference;
                    }
                    else
                    {
                        SelectionHistoryWindowUtils.PingEntry(entry);
                    }
                });
                dragArea.RegisterCallback<MouseDownEvent>(evt =>
                {
                    var entry = selectionHistory.GetEntry(historyIndex);
                    if (entry == null)
                    {
                        return;
                    }
                    
                    if (evt.button == 0 && evt.modifiers.HasFlag(EventModifiers.Alt))
                    {
                        DragAndDrop.PrepareStartDrag();

                        var objectReferences = new[] { entry.Reference };
                        DragAndDrop.paths = new[]
                        {
                            AssetDatabase.GetAssetPath(entry.Reference)
                        };

                        DragAndDrop.objectReferences = objectReferences;
                        DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(entry.Reference));
                    }
                });

                dragArea.RegisterCallback<DragUpdatedEvent>(evt =>
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                });

                dragArea.RegisterCallback<PointerDownEvent>(evt =>
                {
                    var entry = selectionHistory.GetEntry(historyIndex);
                    if (entry == null)
                    {
                        return;
                    }
                    
                    if (evt.button == 0 && evt.clickCount == 2)
                    {
                        if (entry.IsPrefabAsset())
                        {
                            AssetDatabase.OpenAsset(entry.Reference);
                        }
                        
                        if (entry.IsSceneAsset())
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
                            }
                        }

                        if (entry.isUnloadedHierarchyObject)
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                EditorSceneManager.OpenScene(entry.scenePath);
                            }
                        }
                    }
                });
            }
            
            var pingIcon = elementTree.Q<Image>("PingIcon");
            if (pingIcon != null)
            {
                pingIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.searchIconName).image;
                pingIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);
                    if (entry == null)
                    {
                        return;
                    }
                    SelectionHistoryWindowUtils.PingEntry(entry);
                });
            }
            
            var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
            if (openPrefabIcon != null)
            {
                openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openPrefabIconName).image;
                
                openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);

                    if (entry == null)
                    {
                        return;
                    }
                    
                    if (entry.IsPrefabAsset())
                    {
                        AssetDatabase.OpenAsset(entry.Reference);
                    } else if (entry.IsSceneAsset())
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
                        }
                    } else if (entry.isUnloadedHierarchyObject)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(entry.scenePath);
                        }
                    }
                });
            }

            var favoriteAsset = elementTree.Q<Image>("Favorite");
            if (favoriteAsset != null)
            {
                favoriteAsset.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);

                    if (entry == null)
                        return;
                        
                    if (FavoritesController.Favorites.IsFavorite(entry.Reference))
                    {
                        FavoritesController.Favorites.RemoveFavorite(entry.Reference);
                    } else
                    {
                        FavoritesController.Favorites.AddFavorite(new Favorites.Favorite
                        {
                            reference = entry.Reference
                        });
                    }
                            
                    ReloadRootAndRemoveUnloadedAndDuplicated();
                });
            }

            return elementTree;
        }

        private void OnSelectionChanged()
        {
            if (SelectionHistoryWindowUtils.RecordInTheBackground)
            {
                return;
            }

            SelectionHistoryWindowUtils.RecordSelectionChange();
        }

        private void OnHistoryEntryAdded(SelectionHistory selectionHistory)
        {
            ReloadRootAndRemoveUnloadedAndDuplicated();
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ReloadRootAndRemoveUnloadedAndDuplicated();
        }

        public void ReloadRootAndRemoveUnloadedAndDuplicated()
        {
            if (SelectionHistoryWindowUtils.AutomaticRemoveDestroyed)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
            
            if (SelectionHistoryWindowUtils.AutomaticRemoveUnloaded)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);

            if (!SelectionHistoryWindowUtils.AllowDuplicatedEntries)
                selectionHistory.RemoveDuplicated();
            
            ReloadRoot();
        }

        private void ReloadRoot()
        {
            if (visualElements.Count != selectionHistory.historySize)
            {
                RegenerateUI();
            }
            
            var showHierarchyViewObjects =
                EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowHierarchyObjectsPrefKey, true);
            
            var showUnloadedObjects = showHierarchyViewObjects && SelectionHistoryWindowUtils.ShowUnloadedObjects;
            var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects;
            
            var currentEntry = -1;

            if (removeUnloadedButton != null)
            {
                removeUnloadedButton.style.display = showUnloadedObjects ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            if (removeDestroyedButton != null)
            {
                removeDestroyedButton.style.display = showDestroyedObjects ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            for (var i = 0; i < visualElements.Count; i++)
            {
                var visualElement = visualElements[i];
                var entry = selectionHistory.GetEntry(i);
                
                if (entry == null)
                {
                    visualElement.style.display = DisplayStyle.None;
                }
                else
                {
                    currentEntry = i;
                    
                    var isPrefabAsset = entry.isReferenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
                    var isSceneAsset = entry.isReferenced && entry.isAsset && entry.reference is SceneAsset;
                    
                    visualElement.style.display = DisplayStyle.Flex;
                    
                    visualElement.ClearClassList();
                    
                    if (!entry.isReferenced)
                    {
                        visualElement.AddToClassList("unreferencedObject");

                        if (!showDestroyedObjects)
                        {
                            visualElement.style.display = DisplayStyle.None;
                            continue;
                        }
                    }
                    else if (entry.isSceneInstance)
                    {
                        visualElement.AddToClassList("sceneObject");
                    }
                    else
                    {
                        visualElement.AddToClassList("assetObject");
                    }
                    
                    var label = visualElement.Q<Label>("Name");
                    if (label != null)
                    {
                        label.text = entry.GetName(true);
                    }
                    
                    var icon = visualElement.Q<Image>("Icon");
                    if (icon != null)
                    {
                        icon.image = AssetPreview.GetMiniThumbnail(entry.Reference);
                    }
                    
                    var openPrefabIcon = visualElement.Q<Image>("OpenPrefabIcon");
                    if (openPrefabIcon != null)
                    {
                        openPrefabIcon.ClearClassList();
                        
                        if (isPrefabAsset || isSceneAsset || entry.isUnloadedHierarchyObject)
                        {
                            openPrefabIcon.RemoveFromClassList("hidden");
                        }
                        else
                        {
                            openPrefabIcon.AddToClassList("hidden");
                        }
                    }
                    
                    var favoriteAsset = visualElement.Q<Image>("Favorite");
                    if (!SelectionHistoryWindowUtils.ShowFavoriteButton || !entry.isReferenced)
                    {
                        favoriteAsset.style.display = DisplayStyle.None;
                    }
                    else
                    {
                        favoriteAsset.style.display = DisplayStyle.Flex;
                        
                        var isFavorite = FavoritesController.Favorites.IsFavorite(entry.Reference);
                        
                        favoriteAsset.image = isFavorite
                            ? EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteIconName).image
                            : EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteEmptyIconName).image;
                    }
                    
                    var pingIcon = visualElement.Q<Image>("PingIcon");
                    if (pingIcon != null)
                    {
                        if (!entry.isReferenced)
                        {
                            pingIcon.style.display = DisplayStyle.None;
                        }
                        else
                        {
                            pingIcon.style.display = DisplayStyle.Flex;
                        }
                    }
                }
                
                // now update values
                
                // depending configuration, hide elements
            }
            
            if (mainScrollElement != null)
            {
                mainScrollElement.contentContainer.style.flexDirection = SelectionHistoryWindowUtils.OrderLastSelectedFirst ? FlexDirection.ColumnReverse : FlexDirection.Column;

                if (currentEntry >= 0)
                {
                    mainScrollElement.ScrollTo(visualElements[currentEntry]);
                }
            }
        }

        // private VisualElement CreateElementForEntry(SelectionHistory.Entry entry)
        // {
        //     var showHierarchyElements = SelectionHistoryWindowUtils.ShowHierarchyViewObjects;
        //     var showUnloadedObjects = SelectionHistoryWindowUtils.ShowUnloadedObjects;
        //     var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects;
        //     
        //     if (entry.isSceneInstance && !showHierarchyElements)
        //     {
        //         return null;
        //     }
        //
        //     var referenced = entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced;
        //
        //     if (!showUnloadedObjects && entry.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
        //     {
        //         return null;
        //     }
        //
        //     if (!showDestroyedObjects && entry.GetReferenceState() == SelectionHistory.Entry.State.ReferenceDestroyed)
        //     {
        //         return null;
        //     }
        //
        //     var elementTree = historyElementViewTree.CloneTree();
        //     
        //     if (!referenced)
        //     {
        //         elementTree.AddToClassList("unreferencedObject");
        //     }
        //     else if (entry.isSceneInstance)
        //     {
        //         elementTree.AddToClassList("sceneObject");
        //     }
        //     else
        //     {
        //         elementTree.AddToClassList("assetObject");
        //     }
        //
        //     var isPrefabAsset = referenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
        //     var isSceneAsset = referenced && entry.isAsset && entry.reference is SceneAsset;
        //     
        //     if (referenced)
        //     {
        //         var dragArea = elementTree.Q<VisualElement>("DragArea");
        //         if (dragArea != null)
        //         {
        //             dragArea.RegisterCallback<MouseUpEvent>(evt =>
        //             {
        //                 if (evt.button == 0)
        //                 {
        //                     selectionHistory.SetSelection(entry.Reference);
        //                     Selection.activeObject = entry.Reference;
        //                 }
        //                 else
        //                 {
        //                     SelectionHistoryWindowUtils.PingEntry(entry);
        //                 }
        //             });
        //             dragArea.RegisterCallback<MouseDownEvent>(evt =>
        //             {
        //                 if (evt.button == 0 && evt.modifiers.HasFlag(EventModifiers.Alt))
        //                 {
        //                     DragAndDrop.PrepareStartDrag();
        //
        //                     var objectReferences = new[] {entry.Reference};
        //                     DragAndDrop.paths = new[]
        //                     {
        //                         AssetDatabase.GetAssetPath(entry.Reference)
        //                     };
        //
        //                     DragAndDrop.objectReferences = objectReferences;
        //                     DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(entry.Reference));
        //                 }
        //             });
        //
        //             dragArea.RegisterCallback<DragUpdatedEvent>(evt =>
        //             {
        //                 DragAndDrop.visualMode = DragAndDropVisualMode.Link;
        //             });
        //             
        //             dragArea.RegisterCallback<PointerDownEvent>(evt =>
        //             {
        //                 if (evt.button == 0 && evt.clickCount == 2)
        //                 {
        //                     if (isPrefabAsset)
        //                     {
        //                         AssetDatabase.OpenAsset(entry.Reference);
        //                     }
        //                     
        //                     if (isSceneAsset)
        //                     {
        //                         if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        //                         {
        //                             EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
        //                         }
        //                     }
        //                 }
        //             });
        //         }
        //
        //         var icon = elementTree.Q<Image>("Icon");
        //         if (icon != null)
        //         {
        //             icon.image = AssetPreview.GetMiniThumbnail(entry.Reference);
        //         }
        //     }
        //
        //     var pingIcon = elementTree.Q<Image>("PingIcon");
        //     if (pingIcon != null)
        //     {
        //         pingIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.searchIconName).image;
        //         pingIcon.RegisterCallback(delegate(MouseUpEvent e) { SelectionHistoryWindowUtils.PingEntry(entry); });
        //     }
        //     
        //     var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
        //     if (openPrefabIcon != null)
        //     {
        //         openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openPrefabIconName).image;
        //
        //         if (isPrefabAsset || isSceneAsset)
        //         {
        //             openPrefabIcon.RemoveFromClassList("hidden");
        //         }
        //             
        //         openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
        //         {
        //             if (isPrefabAsset)
        //             {
        //                 AssetDatabase.OpenAsset(entry.Reference);
        //             } else if (isSceneAsset)
        //             {
        //                 if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        //                 {
        //                     EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
        //                 }
        //             }
        //         });
        //     }
        //
        //     if (SelectionHistoryWindowUtils.ShowFavoriteButton)
        //     {
        //         if (entry.isAsset &&
        //             entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced)
        //         {
        //             var favoriteAsset = elementTree.Q<Image>("Favorite");
        //             if (favoriteAsset != null)
        //             {
        //                 var isFavorite = FavoritesController.Favorites.IsFavorite(entry.Reference);
        //                 // favoriteEmptyIconName
        //                 favoriteAsset.image = isFavorite
        //                     ? EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteIconName).image
        //                     : EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteEmptyIconName).image;
        //                 favoriteAsset.RegisterCallback(delegate(MouseUpEvent e)
        //                 {
        //                     if (FavoritesController.Favorites.IsFavorite(entry.Reference))
        //                     {
        //                         FavoritesController.Favorites.RemoveFavorite(entry.Reference);
        //                     } else
        //                     {
        //                         FavoritesController.Favorites.AddFavorite(new Favorites.Favorite
        //                         {
        //                             reference = entry.Reference
        //                         });
        //                     }
        //                     
        //                     ReloadRootAndRemoveUnloadedAndDuplicated();
        //                 });
        //             }
        //         }
        //     }
        //
        //     var label = elementTree.Q<Label>("Name");
        //     if (label != null)
        //     {
        //         label.text = entry.GetName(true);
        //     }
        //
        //     return elementTree;
        // }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var showHierarchyViewObjects =
                EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowHierarchyObjectsPrefKey, true);
            
            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowHierarchyObjectsPrefKey, "HierarchyView Objects", 
                "Toggle to show/hide objects from scene hierarchy view.");
		 
            if (showHierarchyViewObjects && !SelectionHistoryWindowUtils.AutomaticRemoveUnloaded)
            {
                AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.ShowUnloadedObjectsKey, "Unloaded Objects", 
                    "Toggle to show/hide unloaded objects from scenes hierarchy view.");
            } 
		    
            if (!SelectionHistoryWindowUtils.AutomaticRemoveDestroyed)
            {
                AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.ShowDestroyedObjectsKey, "Destroyed Objects",
                    "Toggle to show/hide unreferenced or destroyed objects.");
            }
            
            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, "Favorite Button", 
                "Toggle to show/hide favorite Reference button.");
            
            menu.AddItem(new GUIContent("Open preferences"), false, delegate
            {
                SettingsService.OpenUserPreferences("Selection History");
            });
        }

        private void AddMenuItemForPreference(GenericMenu menu, string preference, string text, string tooltip)
        {
            const bool defaultValue = true;
            var value = EditorPrefs.GetBool(preference, defaultValue);
            var name = value ? $"Hide {text}" : $"Show {text}";
            menu.AddItem(new GUIContent(name, tooltip), false, delegate
            {
                ToggleBoolEditorPref(preference, defaultValue);
                ReloadRootAndRemoveUnloadedAndDuplicated();
            });
        }

        private static void ToggleBoolEditorPref(string preferenceName, bool defaultValue)
        {
            var newValue = !EditorPrefs.GetBool(preferenceName, defaultValue);
            EditorPrefs.SetBool(preferenceName, newValue);
            // return newValue;
        }
    }
}
