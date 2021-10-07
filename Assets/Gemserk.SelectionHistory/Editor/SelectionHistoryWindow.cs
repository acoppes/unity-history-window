using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Gemserk
{
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
        
        private void OnDisable()
        {
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded -= OnHistoryEntryAdded;
            }
        }

        public void OnEnable()
        {
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);
            
            selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
            
            if (selectionHistory != null)
            {
                selectionHistory.HistorySize = EditorPrefs.GetInt(SelectionHistoryWindowUtils.HistorySizePrefKey, 10);
                selectionHistory.OnNewEntryAdded += OnHistoryEntryAdded;
            }

            FavoritesController.Favorites.OnFavoritesUpdated += delegate
            {
                ReloadRoot();
            };

            ReloadRoot();
        }

        private void OnHistoryEntryAdded(SelectionHistory history)
        {
            ReloadRoot();
            
            var scroll = rootVisualElement.Q<ScrollView>("MainScroll");
            if (scroll == null) 
                return;
            
            scroll.RegisterCallback(delegate(GeometryChangedEvent evt)
            {
                if (scroll.childCount > 0)
                {
                    var last = scroll.Children().ToList()[scroll.childCount - 1];
                    scroll.ScrollTo(last);
                }
            });
        }

        private void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            ReloadRoot();
        }

        private void OnSceneClosed(Scene scene)
        {
            ReloadRoot();
        }

        private void ReloadRoot()
        {
            var root = rootVisualElement;
            
            root.Clear();
            
            if (SelectionHistoryWindowUtils.AutomaticRemoveDeleted)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

            if (!SelectionHistoryWindowUtils.AllowDuplicatedEntries)
                selectionHistory.RemoveDuplicated();

            var scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "MainScroll"
            };
            
            root.Add(scroll);

            var entries = selectionHistory.History;

            VisualElement lastObject = null;
            
            var showUnloadedObjects = SelectionHistoryWindowUtils.ShowUnloadedObjects;
            var showDestroyedObjects = SelectionHistoryWindowUtils.ShowDestroyedObjects;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                var elementTree = CreateElementForEntry(entry);
                if (elementTree != null)
                {
                    scroll.Add(elementTree);
                }
            }

            var clearButton = new Button(delegate
            {
                selectionHistory.Clear();
                ReloadRoot();
            }) {text = "Clear"};
            root.Add(clearButton);

            if (showUnloadedObjects)
            {
                var removeUnloadedButton = new Button(delegate
                {
                    selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);
                    ReloadRoot();
                }) {text = "Remove Unloaded"};
                root.Add(removeUnloadedButton);
            }
            
            if (showDestroyedObjects)
            {
                var removeDestroyedButton = new Button(delegate
                {
                    selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
                    ReloadRoot();
                }) {text = "Remove destroyed"};
                root.Add(removeDestroyedButton);
            }
            
            // if debug enabled 

            // var button = new Button(delegate
            // {
            //     var scrollOffset = scroll.scrollOffset;
            //     scrollOffset.y = float.MaxValue;
            //     scroll.scrollOffset = scrollOffset;
            // });
            // button.text = "Scroll To Last";
            // root.Add(button);
            //
            // var button2 = new Button(delegate
            // {
            //     var scrollOffset = scroll.scrollOffset;
            //     scrollOffset.y = 0;
            //     scroll.scrollOffset = scrollOffset;
            // });
            // button2.text = "Scroll To First";
            // root.Add(button2);

            //
            // if (allowDuplicatedEntries) {
            //     if (GUILayout.Button ("Remove Duplicated")) {
            //         selectionHistory.RemoveDuplicated ();
            //         Repaint();
            //     }
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

            if (referenced)
            {
                var dragArea = elementTree.Q<VisualElement>("DragArea");
                if (dragArea != null)
                {
                    dragArea.RegisterCallback<MouseUpEvent>(evt =>
                    {
                        if (evt.button == 0)
                        {
                            selectionHistory.SetSelection(entry.reference);
                            Selection.activeObject = entry.reference;
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

                            var objectReferences = new[] {entry.reference};
                            DragAndDrop.paths = new[]
                            {
                                AssetDatabase.GetAssetPath(entry.reference)
                            };

                            DragAndDrop.objectReferences = objectReferences;
                            DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(entry.reference));
                        }
                    });

                    dragArea.RegisterCallback<DragUpdatedEvent>(evt =>
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                    });
                }

                var icon = elementTree.Q<Image>("Icon");
                if (icon != null)
                {
                    icon.image = AssetPreview.GetMiniThumbnail(entry.reference);
                }
            }

            var pingIcon = elementTree.Q<Image>("PingIcon");
            if (pingIcon != null)
            {
                pingIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.searchIconName).image;
                pingIcon.RegisterCallback(delegate(MouseUpEvent e) { SelectionHistoryWindowUtils.PingEntry(entry); });
            }

            if (SelectionHistoryWindowUtils.ShowFavoriteButton)
            {
                if (entry.isAsset &&
                    entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced)
                {
                    var favoriteAsset = elementTree.Q<Image>("Favorite");
                    if (favoriteAsset != null)
                    {
                        var isFavorite = FavoritesController.Favorites.IsFavorite(entry.reference);
                        // favoriteEmptyIconName
                        favoriteAsset.image = isFavorite
                            ? EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteIconName).image
                            : EditorGUIUtility.IconContent(UnityBuiltInIcons.favoriteEmptyIconName).image;
                        favoriteAsset.RegisterCallback(delegate(MouseUpEvent e)
                        {
                            FavoritesController.Favorites.AddFavorite(new Favorites.Favorite
                            {
                                reference = entry.reference
                            });
                            ReloadRoot();
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
                "Toggle to show/hide favorite reference button.");
            
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
                ReloadRoot();
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
