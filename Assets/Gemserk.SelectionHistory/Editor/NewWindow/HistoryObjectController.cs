using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk.Editor
{
    public class HistoryObjectController
    {
        private static readonly string SelectionNormalClass = "selection-normal";
        private static readonly string SelectionCurrentClass = "selection-current";
        private static readonly string SelectionInHierarchyClass = "selection-in-hierarchy";
        
        private VisualElement _root;
        private DraggableLabel _label;
        private Image _thumbnail;
        private Button _pingButton;
        
        private Object _selectionObject;

        public VisualElement Root => _root;
        
        public Object SelectionObject => _selectionObject;
        
        public HistoryObjectController(Object selectionObject, VisualElement selection)
        {
            _root = selection;
            
            _selectionObject = selectionObject;
            
            var labelParent = selection.Q<VisualElement>("HistoryObjectLabel");

            _label = new DraggableLabel();
            _label.SetObjectReferences(selectionObject);
            _label.name = "ObjectName";
            _label.text = selectionObject.name;
            _label.RegisterCallback<MouseUpEvent>(OnMouseUp);
            
            labelParent.Add(_label);

            _thumbnail = selection.Q<Image>("ObjectThumbnail");

            _pingButton = selection.Q<Button>();
            _pingButton.text = "Ping";
            _pingButton.clickable.clicked += PingHistoryObject;

            RefreshThumbnail();
            RefreshLabel();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
                Selection.activeObject = _selectionObject;
            if (evt.button == 1)
                PingHistoryObject();
        }

        private void PingHistoryObject()
        {
            EditorGUIUtility.PingObject(_selectionObject);
        }

        private void RefreshLabel()
        {
            _label.RemoveFromClassList(SelectionNormalClass);
            _label.RemoveFromClassList(SelectionCurrentClass);
            _label.RemoveFromClassList(SelectionInHierarchyClass);
            
            if (Selection.activeObject == _selectionObject)
            {
                _label.AddToClassList(SelectionCurrentClass);
            } else if (!EditorUtility.IsPersistent(_selectionObject))
            {
                _label.AddToClassList(SelectionInHierarchyClass);
            }
            else
            {
                _label.AddToClassList(SelectionNormalClass);
            }
        }

        private void RefreshThumbnail()
        {
            _thumbnail.image = AssetPreview.GetMiniThumbnail(_selectionObject);
        }

        public void Update()
        {
            if (_selectionObject == null) 
                return;
            RefreshLabel();
        }
    }
}