using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private StyleSheet styleSheet;

        private VisualTreeAsset searchToolbarViewTree;
        private VisualTreeAsset historyElementViewTree;

        private SelectionHistory selectionHistory;

        private ScrollView mainScrollElement;
        private List<VisualElement> visualElements = new List<VisualElement>();

        private Button removeUnloadedButton;
        private Button removeDestroyedButton;

        private string searchText;

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
            styleSheet = AssetDatabaseExt.FindAssets(typeof(StyleSheet), "SelectionHistoryStylesheet")
                .OfType<StyleSheet>().FirstOrDefault();
            searchToolbarViewTree = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "SearchToolbar")
                .OfType<VisualTreeAsset>().FirstOrDefault();
            historyElementViewTree = AssetDatabaseExt.FindAssets(typeof(VisualTreeAsset), "SelectionHistoryElement")
                .OfType<VisualTreeAsset>().FirstOrDefault();
            
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded += OnHistoryEntryAdded;
            }

            FavoritesAsset.instance.OnFavoritesUpdated += delegate
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

            root.Add(CreateSearchToolbar());
            
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

        private VisualElement CreateHistoryVisualElement(int index)
        {
            var elementTree = historyElementViewTree.CloneTree();
            var selectionElementRoot = elementTree.Q<VisualElement>("Root");
            
            var historyIndex = index;
            
            var dragArea = selectionElementRoot.Q<VisualElement>("DragArea");
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
                    
                    if (evt.button == 0)
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
            
            var pingIcon = selectionElementRoot.Q<Image>("PingIcon");
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
            
            var openPrefabIcon = selectionElementRoot.Q<Image>("OpenPrefabIcon");
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

            var favoriteAsset = selectionElementRoot.Q<Image>("Favorite");
            if (favoriteAsset != null)
            {
                favoriteAsset.RegisterCallback(delegate(MouseUpEvent e)
                {
                    var entry = selectionHistory.GetEntry(historyIndex);

                    if (entry == null)
                        return;
                        
                    if (FavoritesAsset.instance.IsFavorite(entry.Reference))
                    {
                        FavoritesAsset.instance.RemoveFavorite(entry.Reference);
                    } else
                    {
                        FavoritesAsset.instance.AddFavorite(new FavoritesAsset.Favorite
                        {
                            reference = entry.Reference
                        });
                    }
                            
                    ReloadRootAndRemoveUnloadedAndDuplicated();
                });
            }

            return selectionElementRoot;
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
            
            var showUnloadedObjects = showHierarchyViewObjects && SelectionHistoryWindowUtils.ShowUnloadedObjects 
                                                               && !SelectionHistoryWindowUtils.AutomaticRemoveUnloaded;
            var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects && !SelectionHistoryWindowUtils.AutomaticRemoveDestroyed;
            
            var currentEntry = -1;

            if (removeUnloadedButton != null)
            {
                removeUnloadedButton.style.display = showUnloadedObjects ? DisplayStyle.Flex : DisplayStyle.None;
            }
            
            if (removeDestroyedButton != null)
            {
                removeDestroyedButton.style.display = showDestroyedObjects ? DisplayStyle.Flex : DisplayStyle.None;
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
                    var testName = entry.GetName(false).ToLower();
                    
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
                            visualElement.style.display = DisplayStyle.None;
                            continue;
                        }
                    }
                    
                    currentEntry = i;
                    
                    var isPrefabAsset = entry.isReferenced && entry.isAsset && PrefabUtility.IsPartOfPrefabAsset(entry.Reference) && entry.Reference is GameObject;
                    var isSceneAsset = entry.isReferenced && entry.isAsset && entry.reference is SceneAsset;
                    
                    visualElement.style.display = DisplayStyle.Flex;
                    
                    // since now I am using the root element, remove each specific class to avoid
                    // losing the base ones defined in the uxml file.
                    
                    visualElement.RemoveFromClassList("unreferencedObject");
                    visualElement.RemoveFromClassList("sceneObject");
                    visualElement.RemoveFromClassList("assetObject");

                    // visualElement.AddToClassList("history");
                    
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
                        
                        var isFavorite = FavoritesAsset.instance.IsFavorite(entry.Reference);
                        
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
            
            // menu.AddItem(new GUIContent("Reload UI"), false, delegate
            // {
            //     RegenerateUI();
            // });
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
