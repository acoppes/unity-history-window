using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk.Editor
{
    public class DraggableLabel : Label
    {
        public static string s_DragDataType = "DraggableLabel";
 
        enum DragState
        {
            AtRest,
            Ready,
            Dragging
        }
 
        private DragState m_DragState;
        
        private Object[] m_objectReferences;

        public void SetObjectReferences(Object[] references)
        {
            m_objectReferences = references;
        }
 
        public DraggableLabel()
        {
            m_DragState = DragState.AtRest;
 
            RegisterCallback<MouseDownEvent>(OnMouseDownEvent);
            RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
        }
 
        void OnMouseDownEvent(MouseDownEvent e)
        {
            if (e.target == this && e.button == 0)
            {
                PrepareDraggingBox();
            }
        }
 
        public void PrepareDraggingBox()
        {
            m_DragState = DragState.Ready;
        }
 
        void OnMouseMoveEvent(MouseMoveEvent e)
        {
            if (m_DragState == DragState.Ready)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(s_DragDataType, this);
                DragAndDrop.StartDrag(text);
                DragAndDrop.objectReferences = m_objectReferences;
                StartDragging();
            }
        }
 
        void OnMouseUpEvent(MouseUpEvent e)
        {
            if (m_DragState == DragState.Ready && e.button == 0)
            {
                StopDragging();
            }
        }
 
        private void StartDragging()
        {
           //  AddToClassList("dragged");
            m_DragState = DragState.Dragging;
        }
 
        private void StopDragging()
        {
            // RemoveFromClassList("dragged");
            m_DragState = DragState.AtRest;
        }
    }
}