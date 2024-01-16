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
            
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);
            
            selectionHistory = SelectionHistoryReference.SelectionHistory;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded += OnHistoryEntryAdded;
            }

            FavoritesController.Favorites.OnFavoritesUpdated += delegate
            {
                ReloadRootAndRemoveUnloadedAndDuplicated();
            };
            
            mainScrollElement = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "MainScroll"
            };
            
            // mainScrollElement.RegisterCallback(delegate(GeometryChangedEvent evt)
            // {
            //     var selectedIndex = selectionHistory.GetSelectedIndex();
            //     mainScrollElement.ScrollTo(visualElements[selectedIndex]);
            //     
            //     // if (scroll.childCount > 0)
            //     // {
            //     //     if (SelectionHistoryWindowUtils.OrderLastSelectedFirst)
            //     //     {
            //     //         var first = scroll.Children().ToList().First();
            //     //         scroll.ScrollTo(first);
            //     //     }  else
            //     //     {
            //     //         var last = scroll.Children().ToList()[scroll.childCount - 1];
            //     //         scroll.ScrollTo(last);
            //     //     }
            //     // }
            // });
            
            root.Add(mainScrollElement);
            
            CreateMaxElements(selectionHistory, mainScrollElement);
            
            var showUnloadedObjects = SelectionHistoryWindowUtils.ShowUnloadedObjects;
            var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects;
            
            var clearButton = new Button(delegate
            {
                selectionHistory.Clear();
                ReloadRoot();
            }) {text = "Clear"};
            
            root.Add(clearButton);
            
            // this is just for development
            var refreshButton = new Button(delegate
            {
                ReloadRoot();
            }) {text = "Refresh (dev)"};
            
            root.Add(refreshButton);
            
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
            
            ReloadRootAndRemoveUnloadedAndDuplicated();
            
            Selection.selectionChanged += OnSelectionChanged;
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
                    if (entry == null)
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
                    
                    var isPrefabAsset = entry.isReferenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
                    var isSceneAsset = entry.isReferenced && entry.isAsset && entry.reference is SceneAsset;
                    
                    if (evt.button == 0 && evt.clickCount == 2)
                    {
                        if (isPrefabAsset)
                        {
                            AssetDatabase.OpenAsset(entry.Reference);
                        }

                        if (isSceneAsset)
                        {
                            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                            {
                                EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
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

                // This should be done in update
                
                // if (isPrefabAsset || isSceneAsset)
                // {
                //     openPrefabIcon.RemoveFromClassList("hidden");
                // }
                    
                openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);

                    if (entry == null)
                    {
                        return;
                    }
                    
                    var isPrefabAsset = entry.isReferenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
                    var isSceneAsset = entry.isReferenced && entry.isAsset && entry.reference is SceneAsset;
                    
                    if (isPrefabAsset)
                    {
                        AssetDatabase.OpenAsset(entry.Reference);
                    } else if (isSceneAsset)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
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

            // var scroll = mainScrollElement;
            //
            // var selectedIndex = selectionHistory.GetSelectedIndex();
            // // scroll.ScrollTo(visualElements[selectedIndex]);
            //
            // scroll.RegisterCallback(delegate(GeometryChangedEvent evt)
            // {
            //     scroll.ScrollTo(visualElements[selectedIndex]);
            //     
            //     // if (scroll.childCount > 0)
            //     // {
            //     //     if (SelectionHistoryWindowUtils.OrderLastSelectedFirst)
            //     //     {
            //     //         var first = scroll.Children().ToList().First();
            //     //         scroll.ScrollTo(first);
            //     //     }  else
            //     //     {
            //     //         var last = scroll.Children().ToList()[scroll.childCount - 1];
            //     //         scroll.ScrollTo(last);
            //     //     }
            //     // }
            // });
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ReloadRootAndRemoveUnloadedAndDuplicated();
            //ReloadRoot();
        }

        public void ReloadRootAndRemoveUnloadedAndDuplicated()
        {
            if (SelectionHistoryWindowUtils.AutomaticRemoveDeleted)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

            if (!SelectionHistoryWindowUtils.AllowDuplicatedEntries)
                selectionHistory.RemoveDuplicated();
            
            ReloadRoot();
        }

        private void ReloadRoot()
        {
            var showUnloadedObjects = SelectionHistoryWindowUtils.ShowUnloadedObjects;
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
                        
                        if (isPrefabAsset || isSceneAsset)
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
            
            return;
            
            // var root = rootVisualElement;
            //
            // root.Clear();
            //
            // var scroll = new ScrollView(ScrollViewMode.Vertical)
            // {
            //     name = "MainScroll"
            // };
            //
            // root.Add(scroll);
            //
            // var entries = new List<SelectionHistory.Entry>(selectionHistory.History);
            //
            // var showUnloadedObjects = SelectionHistoryWindowUtils.ShowUnloadedObjects;
            // var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects;
            //
            // if (SelectionHistoryWindowUtils.OrderLastSelectedFirst)
            // {
            //     entries.Reverse();
            // }
            //
            // for (var i = 0; i < entries.Count; i++)
            // {
            //     var entry = entries[i];
            //
            //     var elementTree = CreateElementForEntry(entry);
            //     if (elementTree != null)
            //     {
            //         scroll.Add(elementTree);
            //     }
            // }
            //
            // var clearButton = new Button(delegate
            // {
            //     selectionHistory.Clear();
            //     ReloadRoot();
            // }) {text = "Clear"};
            //
            // root.Add(clearButton);
            //
            // if (showUnloadedObjects)
            // {
            //     var removeUnloadedButton = new Button(delegate
            //     {
            //         selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);
            //         // ReloadRootAndRemoveUnloadedAndDuplicated();
            //         ReloadRoot();
            //     }) {text = "Remove Unloaded"};
            //     root.Add(removeUnloadedButton);
            // }
            //
            // if (showDestroyedObjects)
            // {
            //     var removeDestroyedButton = new Button(delegate
            //     {
            //         selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
            //         // ReloadRootAndRemoveUnloadedAndDuplicated();
            //         ReloadRoot();
            //     }) {text = "Remove destroyed"};
            //     root.Add(removeDestroyedButton);
            // }
        }

        private VisualElement CreateElementForEntry(SelectionHistory.Entry entry)
        {
            var showHierarchyElements = SelectionHistoryWindowUtils.ShowHierarchyViewObjects;
            var showUnloadedObjects = SelectionHistoryWindowUtils.ShowUnloadedObjects;
            var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects;
            
            if (entry.isSceneInstance && !showHierarchyElements)
            {
                return null;
            }

            var referenced = entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced;

            if (!showUnloadedObjects && entry.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
            {
                return null;
            }

            if (!showDestroyedObjects && entry.GetReferenceState() == SelectionHistory.Entry.State.ReferenceDestroyed)
            {
                return null;
            }

            var elementTree = historyElementViewTree.CloneTree();
            
            if (!referenced)
            {
                elementTree.AddToClassList("unreferencedObject");
            }
            else if (entry.isSceneInstance)
            {
                elementTree.AddToClassList("sceneObject");
            }
            else
            {
                elementTree.AddToClassList("assetObject");
            }

            var isPrefabAsset = referenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
            var isSceneAsset = referenced && entry.isAsset && entry.reference is SceneAsset;
            
            if (referenced)
            {
                var dragArea = elementTree.Q<VisualElement>("DragArea");
                if (dragArea != null)
                {
                    dragArea.RegisterCallback<MouseUpEvent>(evt =>
                    {
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
                        if (evt.button == 0 && evt.modifiers.HasFlag(EventModifiers.Alt))
                        {
                            DragAndDrop.PrepareStartDrag();

                            var objectReferences = new[] {entry.Reference};
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
                        if (evt.button == 0 && evt.clickCount == 2)
                        {
                            if (isPrefabAsset)
                            {
                                AssetDatabase.OpenAsset(entry.Reference);
                            }
                            
                            if (isSceneAsset)
                            {
                                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                                {
                                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
                                }
                            }
                        }
                    });
                }

                var icon = elementTree.Q<Image>("Icon");
                if (icon != null)
                {
                    icon.image = AssetPreview.GetMiniThumbnail(entry.Reference);
                }
            }

            var pingIcon = elementTree.Q<Image>("PingIcon");
            if (pingIcon != null)
            {
                pingIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.searchIconName).image;
                pingIcon.RegisterCallback(delegate(MouseUpEvent e) { SelectionHistoryWindowUtils.PingEntry(entry); });
            }
            
            var openPrefabIcon = elementTree.Q<Image>("OpenPrefabIcon");
            if (openPrefabIcon != null)
            {
                openPrefabIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.openPrefabIconName).image;

                if (isPrefabAsset || isSceneAsset)
                {
                    openPrefabIcon.RemoveFromClassList("hidden");
                }
                    
                openPrefabIcon.RegisterCallback(delegate(MouseUpEvent e)
                {
                    if (isPrefabAsset)
                    {
                        AssetDatabase.OpenAsset(entry.Reference);
                    } else if (isSceneAsset)
                    {
                        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        {
                            EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(entry.reference));
                        }
                    }
                });
            }

            if (SelectionHistoryWindowUtils.ShowFavoriteButton)
            {
                if (entry.isAsset &&
                    entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced)
                {
                    var favoriteAsset = elementTree.Q<Image>("Favorite");
                    if (favoriteAsset != null)
                    {
                        var isFavorite = FavoritesController.Favorites.IsFavorite(entry.Reference);
                        // favoriteEmptyIconName
                        favoriteAsset.image = isFavorite
                            ? EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteIconName).image
                            : EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteEmptyIconName).image;
                        favoriteAsset.RegisterCallback(delegate(MouseUpEvent e)
                        {
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
                }
            }

            var label = elementTree.Q<Label>("Name");
            if (label != null)
            {
                label.text = entry.GetName(true);
            }

            return elementTree;
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var showHierarchyViewObjects =
                EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowHierarchyObjectsPrefKey, true);
            
            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.HistoryShowHierarchyObjectsPrefKey, "HierarchyView Objects", 
                "Toggle to show/hide objects from scene hierarchy view.");
		 
            if (showHierarchyViewObjects)
            {
                AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.ShowUnloadedObjectsKey, "Unloaded Objects", 
                    "Toggle to show/hide unloaded objects from scenes hierarchy view.");
            } 
		    
            AddMenuItemForPreference(menu, SelectionHistoryWindowUtils.ShowDestroyedObjectsKey, "Destroyed Objects", 
                "Toggle to show/hide unreferenced or destroyed objects.");
            
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
