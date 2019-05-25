using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk.Editor
{
    public class HistoryItemDragManipulator : MouseManipulator
    {
        private bool m_Active;

        private Object _historyItem;

        public HistoryItemDragManipulator(Object objectAdded)
        {
            _historyItem = objectAdded;
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            m_Active = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            Debug.Log("on mouse down");
            
            if (m_Active)
            {
                Debug.Log("on mouse down and active");
                e.StopImmediatePropagation();
                return;
            }
            
            if (CanStartManipulation(e))
            {
                Debug.Log("on mouse down and can start manipulation");
                m_Active = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!m_Active || !target.HasMouseCapture())
                return;
        
            Debug.Log("on mouse drag");
            
            var historyItem = _historyItem;
            
            DragAndDrop.PrepareStartDrag ();
            DragAndDrop.StartDrag (historyItem.name);
            DragAndDrop.objectReferences = new Object[] { historyItem };
            
            if (EditorUtility.IsPersistent(historyItem)) {

                DragAndDrop.paths = new string[] {
                    AssetDatabase.GetAssetPath(historyItem)
                };
            }

            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (!m_Active || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;

            m_Active = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }
}