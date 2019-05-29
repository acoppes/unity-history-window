using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIElementsExamples
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
        
        public Object[] m_objectReferences;
 
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
                StartDraggingBox();
            }
        }
 
        void OnMouseUpEvent(MouseUpEvent e)
        {
            if (m_DragState == DragState.Ready && e.button == 0)
            {
                StopDraggingBox();
            }
        }
 
        public void StartDraggingBox()
        {
            AddToClassList("dragged");
            m_DragState = DragState.Dragging;
        }
 
        public void StopDraggingBox()
        {
            RemoveFromClassList("dragged");
            m_DragState = DragState.AtRest;
        }
    }
}