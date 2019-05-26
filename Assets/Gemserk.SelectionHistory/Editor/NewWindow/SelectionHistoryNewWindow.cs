using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace Gemserk.Editor
{
    public class SelectionItemElement
    {
        private VisualElement _label;
//        private ObjectField _objectField;

        private StyleColor _previousColor;

        private Object _selectionObject;
        
        public SelectionItemElement(Object selectionObject, VisualElement selection)
        {
            _selectionObject = selectionObject;
            _label = selection.Q<Label>("ObjectName");
            _previousColor = _label.style.color;
            
            Selection.selectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged()
        {
            //var icon = AssetPreview.GetMiniThumbnail(obj);
            _label.style.color = _previousColor;
            if (Selection.activeObject == _selectionObject)
            {
                _label.style.color = new StyleColor(Color.blue);
            }
        }
    }
    
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
        
        private List<SelectionItemElement> _selections = new List<SelectionItemElement>();

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
            
            var clearButton = new Button(delegate
            {
                selectionHistory.Clear();
                // _historyObjectsContainer.Clear();
                // clear list too
            });
            clearButton.text = "Clear";
            
            root.Add(clearButton);
            
            var scheduledAction = root.schedule.Execute(() =>
            {
                // textFields.ForEach(t => t.value = m_Tank.tankName);
                // integerFields.ForEach(t => t.value = m_Tank.tankSize);
            });
            scheduledAction.Every(100); // ms

            selectionHistory.objectAdded += AddSelectionField;
            selectionHistory.cleared += () =>
            {
                _historyObjectsContainer.Clear();
            };

            selectionHistory.History.ForEach(AddSelectionField);
        }

        public void OnDisable()
        {
            selectionHistory.objectAdded -= AddSelectionField;
            // Selection.selectionChanged -= OnSelectionChanged;
        }

        private void AddSelectionField(Object objectAdded)
        {
            // if object field with object added already, remove it...

//            var previous = _historyObjectsContainer.Query<ObjectField>().Where(field => field.value == objectAdded).ToList();
//
//            if (previous.Count > 0)
//            {
//                var previousField = previous[0];
//
//                var objectFieldSelection = _historyObjectsContainer.Query<VisualElement>(SelectionContainerName)
//                    .Where(s => s.Q<ObjectField>() == previousField).First();
//                
//                if (objectFieldSelection != null)
//                {
//                    _historyObjectsContainer.Remove(objectFieldSelection);
//                    _historyObjectsContainer.Add(objectFieldSelection);
//                }
//                
//                return;
//            }
            
            var tree = _visualTreeAsset.CloneTree();
            var selectionElement = tree.Q(SelectionContainerName);

            var button = selectionElement.Q<Button>();
            button.text = "Ping";
            
            // var objectField = selectionContainer.Q<ObjectField>( );
            // objectField.pickingMode = PickingMode.Ignore;
            // objectField.SetValueWithoutNotify(objectAdded);

            button.clickable.clicked += delegate
            {
                EditorGUIUtility.PingObject(objectAdded);
            };
            
            // selectionContainer.Q<Label>("Dragger").AddManipulator(new HistoryItemDragManipulator(this, objectAdded));

            var objectName = selectionElement.Q<Label>("ObjectName");
            objectName.text = objectAdded.name;
            
            _historyObjectsContainer.Add(selectionElement);
            
            _selections.Add(new SelectionItemElement(objectAdded, selectionElement));
        }
    }
}