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
        
        private Object m_ObjectReference;

        public void SetObjectReferences(Object reference)
        {
            m_ObjectReference = reference;
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
                PrepareDragging();
            }
        }
 
        public void PrepareDragging()
        {
            m_DragState = DragState.Ready;
            Debug.LogFormat("DraggableLabel: {0}", m_ObjectReference.name);
        }
 
        void OnMouseMoveEvent(MouseMoveEvent e)
        {
            if (m_DragState == DragState.Ready)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(s_DragDataType, this);
                DragAndDrop.StartDrag(m_ObjectReference.name);
                DragAndDrop.objectReferences = new Object[] {m_ObjectReference};
                
                if (EditorUtility.IsPersistent(m_ObjectReference)) {

                    // added DragAndDrop.path in case we are dragging a folder.

                    DragAndDrop.paths = new string[] {
                        AssetDatabase.GetAssetPath(m_ObjectReference)
                    };

                    // previous test with setting generic data by looking at
                    // decompiled Unity code.

                    // DragAndDrop.SetGenericData ("IsFolder", "isFolder");
                }
                
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