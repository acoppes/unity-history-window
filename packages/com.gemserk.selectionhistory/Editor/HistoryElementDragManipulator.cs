using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    // Some links to have as reference for all the drag iterations I had:
    // https://forum.unity.com/threads/how-to-register-drag-and-click-events-on-the-same-visualelement.1189135/
    // https://forum.unity.com/threads/mouse-events-not-working-for-uielements.1100497/
    
    public class HistoryElementDragManipulator : MouseManipulator
    {
        private readonly EventCallback<MouseDownEvent> mouseDownHandler;
        private readonly EventCallback<MouseMoveEvent> mouseMoveHandler;
        private readonly EventCallback<MouseUpEvent> mouseUpHandler;
        private readonly EventCallback<ClickEvent> mouseClickHandler;
     
        private Vector2 startPosition;
        private bool isDragging;
        private bool isPressed;

        private readonly SelectionHistory selectionHistory;
        private readonly int historyIndex;
     
        public HistoryElementDragManipulator(SelectionHistory selectionHistory, int historyIndex)
        {
            this.selectionHistory = selectionHistory;
            this.historyIndex = historyIndex;
            
            mouseDownHandler = OnMouseDown;
            mouseMoveHandler = OnMouseMove;
            mouseUpHandler = OnMouseUp;
            mouseClickHandler = OnClickEvent;
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            startPosition = evt.mousePosition;
            isPressed = true;
            isDragging = false;
     
            // Capturing the pointer in case of overlapping to ensure we get the pointer up event even if the pointer
            // moved outside of the target.
            target.CapturePointer(0);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            isPressed = false;
            
            target.ReleasePointer(0);
            
            if (isDragging)
            {
                return;
            }
            
            var entry = selectionHistory.GetEntry(historyIndex);    
            
            if (evt.button == 0 && evt.clickCount == 1)
            {
                // Just select the object
                selectionHistory.SetSelection(entry.Reference);
                Selection.activeObject = entry.Reference;
            }
            
            if (evt.button == 1)
            {
                // Just ping the object
                SelectionHistoryWindowUtils.PingEntry(entry);
            } 
        }
        
        private void OnClickEvent(ClickEvent evt)
        {
            isPressed = false;
            
            target.ReleasePointer(0);
            
            if (isDragging)
            {
                return;
            }
            
            var entry = selectionHistory.GetEntry(historyIndex);    
            
            if (evt.button == 0 && evt.clickCount == 2)
            {
                // Try to open the asset.
                
                if (entry.isAsset || entry.IsSceneAsset())
                {
                    AssetDatabase.OpenAsset(entry.Reference);
                }

                if (entry.isUnloadedHierarchyObject)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(entry.scenePath);
                    }
                }
            }
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isPressed)
            {
                return;
            }
            
            if (isDragging)
            {
                // dont want to restart the drag all the time
                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                return;
            }
            
            // if not moved enough, just ignore.
            var distance = SelectionHistoryWindowUtils.distanceToConsiderDrag;
            if ((startPosition - evt.mousePosition).sqrMagnitude < distance * distance)
            {
                return;
            }
            
            isDragging = true;
            
            var entry = selectionHistory.GetEntry(historyIndex);
            
            // This is necessary in order to process the drag properly. 
            target.ReleasePointer(0);
            
            DragAndDrop.PrepareStartDrag();
            
            var objectReferences = new[] { entry.Reference };
            DragAndDrop.paths = new[]
            {
                AssetDatabase.GetAssetPath(entry.Reference)
            };
            
            // DragAndDrop.SetGenericData("mousePosition", evt.originalMousePosition);
            // DragAndDrop.SetGenericData("startTime", evt.timestamp);
            
            DragAndDrop.objectReferences = objectReferences;
            DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(entry.Reference));
            DragAndDrop.visualMode = DragAndDropVisualMode.Move;
        }
     
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback(mouseDownHandler);
            target.RegisterCallback(mouseMoveHandler);
            target.RegisterCallback(mouseUpHandler);
            target.RegisterCallback(mouseClickHandler);
        }
     
        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback(mouseDownHandler);
            target.UnregisterCallback(mouseMoveHandler);
            target.UnregisterCallback(mouseUpHandler);
            target.UnregisterCallback(mouseClickHandler);
        }
    }
}