using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Gemserk
{
    public class NewSelectionHistoryWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/Gemserk/New Selection History")]
        public static void OpenWindow()
        {
            var window = GetWindow<NewSelectionHistoryWindow>();
            var titleContent = EditorGUIUtility.IconContent(UnityBuiltInIcons.refreshIconName);
            titleContent.text = "New History";
            titleContent.tooltip = "New objects selection history";
            window.titleContent = titleContent;
        }
        
        public StyleSheet styleSheet;

        public VisualTreeAsset historyElementViewTree;

        private SelectionHistory selectionHistory;
        
        private void OnDisable()
        {
            EditorSceneManager.sceneClosed -= OnSceneClosed;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
        }

        public void OnEnable()
        {
            EditorSceneManager.sceneClosed += OnSceneClosed;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            
            var root = rootVisualElement;
            root.styleSheets.Add(styleSheet);
            
            selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
            
            Selection.selectionChanged += delegate {
                
                ReloadRoot();

                var mainScroll = rootVisualElement.Q<ScrollView>("MainScroll");
                if (mainScroll != null)
                {
                    if (selectionHistory.IsSelected(selectionHistory.GetHistoryCount() - 1))
                    {
                        var scrollOffset = mainScroll.scrollOffset;
                        scrollOffset.y = float.MaxValue;
                        mainScroll.scrollOffset = scrollOffset;
                    }
                }
            };

            ReloadRoot();
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
            
            if (SelectionHistoryWindow.AutomaticRemoveDeleted)
                selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

            if (!SelectionHistoryWindow.AllowDuplicatedEntries)
                selectionHistory.RemoveDuplicated();

            var scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "MainScroll"
            };
            
            root.Add(scroll);

            var entries = selectionHistory.History;

            VisualElement lastObject = null;

            var showHierarchyElements = SelectionHistoryWindow.ShowHierarchyViewObjects;
            var showUnloadedObjects = SelectionHistoryWindow.ShowUnloadedObjects;
            var showDestroyedObjects = SelectionHistoryWindow.ShowDestroyedObjects;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                if (entry.isSceneInstance && !showHierarchyElements)
                {
                    continue;
                }

                var referenced = entry.GetReferenceState() == SelectionHistory.Entry.State.Referenced;
                
                if (!showUnloadedObjects && entry.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
                {
                    continue;
                }
                
                if (!showDestroyedObjects && entry.GetReferenceState() == SelectionHistory.Entry.State.ReferenceDestroyed)
                {
                    continue;
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
#if !UNITY_EDITOR_OSX
                        dragArea.RegisterCallback<MouseDownEvent>(evt =>
                        {
                            if (evt.button == 1)
                            {
                                SelectionHistoryWindow.PingEntry(entry);
                                return;
                            }
                        
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.StartDrag("Dragging");
                            DragAndDrop.objectReferences = new Object[] {entry.reference};
                        });
                        dragArea.RegisterCallback<DragUpdatedEvent>(evt =>
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                        });
#else
                        dragArea.RegisterCallback<MouseDownEvent>(evt =>
                        {
                            SelectionHistoryWindow.PingEntry(entry);
                        });
#endif
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
                    pingIcon.RegisterCallback(delegate(MouseUpEvent e)
                    {
                        SelectionHistoryWindow.PingEntry(entry);
                    });
                }
                
                var label = elementTree.Q<Label>("Name");
                if (label != null)
                {
                    label.text = entry.GetName(true);
                }

                scroll.Add(elementTree);
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
            
            //
            // if (allowDuplicatedEntries) {
            //     if (GUILayout.Button ("Remove Duplicated")) {
            //         selectionHistory.RemoveDuplicated ();
            //         Repaint();
            //     }
            // }
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            var showHierarchyViewObjects =
                EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, true);
            
            AddMenuItemForPreference(menu, SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, "HierarchyView Objects", 
                "Toggle to show/hide objects from scene hierarchy view.");
		 
            if (showHierarchyViewObjects)
            {
                AddMenuItemForPreference(menu, SelectionHistoryWindow.ShowUnloadedObjectsKey, "Unloaded Objects", 
                    "Toggle to show/hide unloaded objects from scenes hierarchy view.");
            } 
		    
            AddMenuItemForPreference(menu, SelectionHistoryWindow.ShowDestroyedObjectsKey, "Destroyed Objects", 
                "Toggle to show/hide unreferenced or destroyed objects.");
            
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