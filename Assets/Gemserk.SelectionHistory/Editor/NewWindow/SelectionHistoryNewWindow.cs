using System;
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

        private static readonly string SelectionContainerName = "Selection";
        
        private static Vector2 _windowMinSize = new Vector2(300, 200);

        [Shortcut("Selection History/Show", null, KeyCode.H, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        [MenuItem("Window/Gemserk/New Selection History")]
        public static void OpenSelectionHistoryWindow()
        {
            var window = GetWindow<SelectionHistoryNewWindow>();
            window.titleContent = new GUIContent("SelectionHistoryNewWindow");
            window.minSize = _windowMinSize;
        }
        
        private static SelectionHistory selectionHistory
        {
            get { return SelectionHistoryContext.SelectionHistory; }
        }

        private StyleSheet _styleSheet;

        private VisualTreeAsset _visualTreeAsset;

        private VisualElement _historyObjectsContainer;
        
        private List<SelectionItemVisualElement> _selections = new List<SelectionItemVisualElement>();

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
            var root = rootVisualElement;

            _historyObjectsContainer = new VisualElement();
            
            root.Add(_historyObjectsContainer);

            _styleSheet = LoadStyleSheet();
            _visualTreeAsset = LoadTreeAsset();

            if (_styleSheet == null || _visualTreeAsset == null)
            {
                Debug.LogError("Failed to initialize selection history");
                return;
            }
            
            root.styleSheets.Add(_styleSheet);
            
            AddClearButton();
            AddPreferencesButton();
            
            var scheduledAction = root.schedule.Execute(OnUpdate);
            scheduledAction.Every(30); // ms

            selectionHistory.objectAdded += AddSelectionField;
            selectionHistory.cleared += () =>
            {
                _selections.Clear();
                _historyObjectsContainer.Clear();
            };

            selectionHistory.History.ForEach(AddSelectionField);
        }

        private void AddClearButton()
        {
            var clearButton = new Button(delegate
            {
                selectionHistory.Clear();
            });
            clearButton.text = "Clear";
            
            rootVisualElement.Add(clearButton);
        }
        
        private void AddPreferencesButton()
        {
            var button = new Button(delegate
            {
                SettingsService.OpenUserPreferences(SelectionHistoryPreferences.PreferencesPath);
            });
            button.text = "Preferences";
            rootVisualElement.Add(button);
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
//                _historyObjectsContainer.Remove(previous.Parent);
//                _historyObjectsContainer.Add(previous.Parent);

                return;
            }
            
            var tree = _visualTreeAsset.CloneTree();
            var selectionElement = tree.Q(SelectionContainerName);

            var button = selectionElement.Q<Button>();
            button.text = "Ping";

            button.clickable.clicked += delegate
            {
                EditorGUIUtility.PingObject(objectAdded);
            };
            
            // selectionContainer.Q<Label>("Dragger").AddManipulator(new HistoryItemDragManipulator(this, objectAdded));

            var objectName = selectionElement.Q<Label>("ObjectName");
            objectName.text = objectAdded.name;
            
            _historyObjectsContainer.Add(selectionElement);
            
            _selections.Add(new SelectionItemVisualElement(objectAdded, selectionElement));
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
                    _historyObjectsContainer.Remove(s.Parent);
                }
            });
            _selections.RemoveAll(s => s.SelectionObject == null);
            
        }
    }
}