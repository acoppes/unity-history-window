using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk.Editor
{
    public class SelectionItemVisualElement
    {
        private VisualElement _root;
        private DraggableLabel _label;
        private StyleColor _previousColor;

        private Object _selectionObject;

        private Image _thumbnail;

        public VisualElement Root => _root;
        
        public Object SelectionObject => _selectionObject;
        
        public SelectionItemVisualElement(Object selectionObject, VisualElement selection)
        {
            _root = selection;
            
            _selectionObject = selectionObject;
            
            var selectionLabel = selection.Q<VisualElement>("SelectionLabel");

            _label = new DraggableLabel();
            _label.SetObjectReferences(new Object[] { selectionObject });
            _label.name = "ObjectName";
            _label.text = selectionObject.name;
            
            selectionLabel.Add(_label);

            // _label = selection.Q<Label>("ObjectName");
            _previousColor = _label.style.color;

            _thumbnail = selection.Q<Image>("ObjectThumbnail");

            RefreshThumbnail();
            
            _label.RegisterCallback<MouseUpEvent>(OnMouseUp);

            RefreshLabel();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
                Selection.activeObject = _selectionObject;
            if (evt.button == 1)
                EditorGUIUtility.PingObject(_selectionObject);
        }

        private void RefreshLabel()
        {
            _label.style.color = _previousColor;
            if (Selection.activeObject == _selectionObject)
            {
                _label.style.color = new StyleColor(SelectionHistoryWindowConstants.selectedElementColor);
            } else if (!EditorUtility.IsPersistent(_selectionObject))
            {
                _label.style.color = new StyleColor(SelectionHistoryWindowConstants.hierarchyElementColor);
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