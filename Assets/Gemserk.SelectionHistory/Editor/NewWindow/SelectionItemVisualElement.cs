using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk.Editor
{
    public class SelectionItemVisualElement
    {
        private VisualElement _parent;
        private VisualElement _label;
        private StyleColor _previousColor;

        private Object _selectionObject;

        private Image _thumbnail;

        public VisualElement Parent => _parent;
        
        public Object SelectionObject => _selectionObject;

        private bool _registered;
        
        public SelectionItemVisualElement(Object selectionObject, VisualElement selection)
        {
            _parent = selection;
            
            _selectionObject = selectionObject;
            _label = selection.Q<Label>("ObjectName");
            _previousColor = _label.style.color;

            _thumbnail = selection.Q<Image>("ObjectThumbnail");

            RefreshThumbnail();

            Selection.selectionChanged += OnSelectionChanged;
            _registered = true;
            
            _label.RegisterCallback<MouseUpEvent>(OnMouseUp);;

            RefreshSelection();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
                Selection.activeObject = _selectionObject;
        }

        private void OnSelectionChanged()
        {
            RefreshSelection();
        }

        private void RefreshSelection()
        {
            _label.style.color = _previousColor;
            if (Selection.activeObject == _selectionObject)
            {
                _label.style.color = new StyleColor(Color.blue);
            }
        }

        private void RefreshThumbnail()
        {
            _thumbnail.image = AssetPreview.GetMiniThumbnail(_selectionObject);
        }

        public void Update()
        {
            if (_selectionObject == null && _registered)
            {
                Selection.selectionChanged -= OnSelectionChanged;
                _registered = false;
            }
        }
    }
}