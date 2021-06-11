using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    public class NewSelectionHistoryWindow : EditorWindow
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

        }

        public void OnEnable()
        {
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
                        // _historyScrollPosition.y = float.MaxValue;
                    }
                }
            };
            
            // root.RegisterCallback<DragPerformEvent>(evt =>
            // {
            //     DragAndDrop.AcceptDrag();
            //     FavoriteElements(DragAndDrop.objectReferences);
            // });
            //
            //  
            // box.RegisterCallback<MouseDownEvent>(evt =>
            // {
            //     DragAndDrop.PrepareStartDrag();
            //     DragAndDrop.StartDrag("Dragging");
            //     DragAndDrop.objectReferences = new Object[] { prefab };
            // });
            
            // root.RegisterCallback<DragUpdatedEvent>(evt =>
            // {
            //     DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            // });
            
            ReloadRoot();
        }

        private void ReloadRoot()
        {
            var root = rootVisualElement;
            
            root.Clear();

            var scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                name = "MainScroll"
            };
            
            root.Add(scroll);

            var entries = selectionHistory.History;

            VisualElement lastObject = null;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                
                var elementTree = historyElementViewTree.CloneTree();

                if (entry.reference != null)
                {
                    var ping = elementTree.Q<VisualElement>("Ping");
                    if (ping != null)
                    {
                        ping.RegisterCallback(delegate(MouseUpEvent e)
                        {
                            EditorGUIUtility.PingObject(entry.reference);
                        });
                    }

                    var icon = elementTree.Q<Image>("Icon");
                    if (icon != null)
                    {
                        icon.image = AssetPreview.GetMiniThumbnail(entry.reference);
                    }
                }
                
                // var removeIcon = elementTree.Q<Image>("RemoveIcon");
                // if (removeIcon != null)
                // {
                //     // removeIcon.image = AssetPreview.GetMiniThumbnail(assetReference);
                //     removeIcon.image = EditorGUIUtility.IconContent(UnityBuiltInIcons.removeIconName).image;
                //     
                //     removeIcon.RegisterCallback(delegate(MouseUpEvent e)
                //     {
                //         FavoritesController.Favorites.RemoveFavorite(assetReference);
                //     });
                // }
                
                var label = elementTree.Q<Label>("Name");
                if (label != null)
                {
                    label.text = entry.GetName(true);
                }

                scroll.Add(elementTree);
                // lastObject = elementTree;
            }

            var clearButton = new Button
            {
                text = "Clear"
            };
            clearButton.clicked += delegate
            {
                selectionHistory.Clear();
                ReloadRoot();
            };
            root.Add(clearButton);

            // if (lastObject != null)
            // {
            //     scroll.scrollOffset = 
            //     scroll.ScrollTo(lastObject);
            // }
        }
    }
}