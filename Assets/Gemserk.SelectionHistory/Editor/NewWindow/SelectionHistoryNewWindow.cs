using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Gemserk.Editor
{
    public class SelectionHistoryNewWindow : EditorWindow
    {
        private static readonly string StyleSheetFileName = "SelectionHistoryNewWindow";
        private static readonly string VisualTreeFileName = "SelectionHistoryNewWindow";

        private static readonly string SelectionContainerName = "HistoryObject";
        
        private static Vector2 _windowMinSize = new Vector2(300, 200);
        
        const string MenuItemOpenWindow = "Window/Gemserk/Selection History";

        const string ShortcutOpenWindow = "Selection History/Show";

        [Shortcut(ShortcutOpenWindow, null, KeyCode.H, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [MenuItem(MenuItemOpenWindow)]
        public static void OpenSelectionHistoryWindow()
        {
            var window = GetWindow<SelectionHistoryNewWindow>();
            window.titleContent = new GUIContent(SelectionHistoryWindowConstants.WindowName);
            window.minSize = _windowMinSize;
        }
        
        private static SelectionHistory selectionHistory => SelectionHistoryContext.SelectionHistory;

        private StyleSheet _styleSheet;

        private VisualTreeAsset _visualTreeAsset;

        private ScrollView _historyObjectsContainer;
        
        private List<HistoryObjectController> _selections = new List<HistoryObjectController>();

        private static StyleSheet LoadStyleSheet()
        {
            var guids = AssetDatabase.FindAssets("t:StyleSheet " + StyleSheetFileName);

            if (guids.Length == 0)
                return null;
            
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        private static VisualTreeAsset LoadTreeAsset()
        {
            var guids = AssetDatabase.FindAssets("t:VisualTreeAsset " + VisualTreeFileName);

            if (guids.Length == 0)
                return null;

            return AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
        
        public void OnEnable()
        {
            _selections.Clear();
            
            // Each editor window contains a root VisualElement object
            
            _historyObjectsContainer = new ScrollView(ScrollViewMode.Vertical);
            
            rootVisualElement.Add(_historyObjectsContainer);

            _styleSheet = LoadStyleSheet();
            _visualTreeAsset = LoadTreeAsset();

            if (_styleSheet == null || _visualTreeAsset == null)
            {
                Debug.LogError("Failed to initialize selection history");
                return;
            }
            
            rootVisualElement.styleSheets.Add(_styleSheet);
            
            AddClearButton();
            AddPreferencesButton();
            
            var scheduledAction = rootVisualElement.schedule.Execute(OnUpdate);
            scheduledAction.Every(30); // ms

            selectionHistory.objectAdded += AddSelectionField;
            selectionHistory.cleared += () =>
            {
                _selections.Clear();
                _historyObjectsContainer.Clear();
            };

            selectionHistory.History.ForEach(AddSelectionField);
            
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            var selectionItem = _selections.FirstOrDefault(s => s.SelectionObject == Selection.activeObject);
            if (selectionItem != null)
            {
                _historyObjectsContainer.ScrollTo(selectionItem.Root);
            }
        }

        private void AddClearButton()
        {
            rootVisualElement.Add(new Button(delegate { selectionHistory.Clear(); })
            {
                text = "Clear"
            });
        }
        
        private void AddPreferencesButton()
        {
            rootVisualElement.Add(new Button(delegate
            {
                SettingsService.OpenUserPreferences(SelectionHistoryPreferences.PreferencesPath);
            })
            {
                text = "Preferences"
            });
        }

        public void OnDisable()
        {
            selectionHistory.objectAdded -= AddSelectionField;
            // Selection.selectionChanged -= OnSelectionChanged;
        }

        private void AddSelectionField(Object objectAdded)
        {
            // if object field with object added already, remove it...

            var previous = _selections.FirstOrDefault(s => s.SelectionObject == objectAdded);

            if (previous != null)
            { 
                _historyObjectsContainer.Remove(previous.Root);
                _historyObjectsContainer.Add(previous.Root);
                _historyObjectsContainer.verticalScroller.value = _historyObjectsContainer.verticalScroller.highValue;
                return;
            }
            
            var tree = _visualTreeAsset.CloneTree();
            var selectionElement = tree.Q(SelectionContainerName);

            _historyObjectsContainer.Add(selectionElement);
            
            _selections.Add(new HistoryObjectController(objectAdded, selectionElement, selectionHistory));
        }

        private void OnUpdate()
        {
            // iterate and remove those with deleted items...
            // if autoremvoe items
            
            // if (automaticRemoveDeleted)
            selectionHistory.ClearDeleted ();
            
            // var deletedItems = _selections.Where(s => s.SelectionObject == null).ToList();
            
            _selections.ForEach(s =>
            {
                s.Update();
                if (s.SelectionObject == null)
                {
                    _historyObjectsContainer.Remove(s.Root);
                }
            });
            _selections.RemoveAll(s => s.SelectionObject == null);
            
        }
    }
}