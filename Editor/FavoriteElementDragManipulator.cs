using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace Gemserk
{
    public class FavoriteElementDragManipulator : MouseManipulator
    {
        private readonly EventCallback<MouseDownEvent> mouseDownHandler;
        private readonly EventCallback<MouseMoveEvent> mouseMoveHandler;
        private readonly EventCallback<MouseUpEvent> mouseUpHandler;
        private readonly EventCallback<ClickEvent> mouseClickHandler;
     
        private Vector2 startPosition;
        private bool isDragging;
        private bool isPressed;

        private readonly Object assetReference;
     
        public FavoriteElementDragManipulator(Object assetReference)
        {
            this.assetReference = assetReference;
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
            
            if (evt.button == 0)
            {
                Selection.activeObject = assetReference;
            }
            
            if (evt.button == 1)
            {
                EditorGUIUtility.PingObject(assetReference);
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
            
            if (evt.button == 0 && evt.clickCount == 2)
            {
                AssetDatabase.OpenAsset(assetReference);
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
            
            // This is necessary in order to process the drag properly. 
            target.ReleasePointer(0);
            
            DragAndDrop.PrepareStartDrag();
            
            var objectReferences = new[] { assetReference };
            DragAndDrop.paths = new[]
            {
                AssetDatabase.GetAssetPath(assetReference)
            };
            
            // DragAndDrop.SetGenericData("mousePosition", evt.originalMousePosition);
            // DragAndDrop.SetGenericData("startTime", evt.timestamp);
            
            DragAndDrop.objectReferences = objectReferences;
            DragAndDrop.StartDrag(ObjectNames.GetDragAndDropTitle(assetReference));
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