using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace UIElementsExamples
{
    public class E18_DragAndDrop : EditorWindow
    {
        [MenuItem("UIElementsExamples/18_DragAndDrop")]
        public static void ShowExample()
        {
            E18_DragAndDrop window = GetWindow<E18_DragAndDrop>();
            window.minSize = new Vector2(450, 514);
            window.titleContent = new GUIContent("Example 18");
        }
 
        private VisualElement m_DropArea;
        private Label m_Ghost;

        private Object[] m_objectReferences;
 
        public void OnEnable()
        {
            var root = rootVisualElement;
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/DragAndDrop/E18_DragAndDrop.uss"));
 
            m_DropArea = new VisualElement();
            m_DropArea.AddToClassList("droparea");
            m_DropArea.Add(new Label {text = "Drag and drop anything here"});
            root.Add(m_DropArea);
 
            m_Ghost = new Label();
            m_Ghost.AddToClassList("ghost");
            m_DropArea.Add(m_Ghost);
 
            m_DropArea.RegisterCallback<DragEnterEvent>(OnDragEnterEvent);
            m_DropArea.RegisterCallback<DragLeaveEvent>(OnDragLeaveEvent);
            m_DropArea.RegisterCallback<DragUpdatedEvent>(OnDragUpdatedEvent);
            m_DropArea.RegisterCallback<DragPerformEvent>(OnDragPerformEvent);
            m_DropArea.RegisterCallback<DragExitedEvent>(OnDragExitedEvent);
        }
 
        void OnDragEnterEvent(DragEnterEvent e)
        {
            m_DropArea.AddToClassList("dragover");
            m_Ghost.AddToClassList("visible");
            m_Ghost.style.left = e.localMousePosition.x - m_Ghost.resolvedStyle.width / 2;
            m_Ghost.style.top = e.localMousePosition.y - m_Ghost.resolvedStyle.height / 2;
            m_Ghost.text = "";
            
            m_objectReferences = null;

            object draggedLabel = DragAndDrop.GetGenericData(DraggableLabel.s_DragDataType);
            if (draggedLabel != null)
            {
                var label = (DraggableLabel)draggedLabel;
                m_Ghost.text = label.text;
 
                // if mouse exited then re-entered drop area, we need to call PrepareDraggingBox again.
                label.PrepareDraggingBox();
 
                label.StartDraggingBox();
            }
            else
            {
                List<string> names = new List<string>();
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    names.Add(obj.name);
                }
 
                m_Ghost.text = String.Join(", ", names);
                m_objectReferences = DragAndDrop.objectReferences;
            }
        }
 
        void OnDragLeaveEvent(DragLeaveEvent e)
        {
            m_DropArea.RemoveFromClassList("dragover");
            m_Ghost.RemoveFromClassList("visible");
 
            object draggedLabel = DragAndDrop.GetGenericData(DraggableLabel.s_DragDataType);
            if (draggedLabel != null)
            {
                var label = (DraggableLabel)draggedLabel;
                label.StopDraggingBox();
            }
        }
 
        void OnDragUpdatedEvent(DragUpdatedEvent e)
        {
            m_Ghost.style.left = e.localMousePosition.x - m_Ghost.resolvedStyle.width / 2;
            m_Ghost.style.top = e.localMousePosition.y - m_Ghost.resolvedStyle.height / 2;
 
            object draggedLabel = DragAndDrop.GetGenericData(DraggableLabel.s_DragDataType);
            if (draggedLabel != null)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Move;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
        }
 
        void OnDragPerformEvent(DragPerformEvent e)
        {
            DragAndDrop.AcceptDrag();
 
            object draggedLabel = DragAndDrop.GetGenericData(DraggableLabel.s_DragDataType);
            if (draggedLabel != null)
            {
                var label = (DraggableLabel)draggedLabel;
                label.style.top = m_Ghost.resolvedStyle.top;
                label.style.left = m_Ghost.resolvedStyle.left;
                label.StopDraggingBox();
            }
            else
            {
                var newBox = new DraggableLabel();
                newBox.AddToClassList("box");
                newBox.style.top = m_Ghost.resolvedStyle.top;
                newBox.style.left = m_Ghost.resolvedStyle.left;
                newBox.text = m_Ghost.text;
                newBox.m_objectReferences = m_objectReferences;
                // Insert before ghost
                m_DropArea.Insert(m_DropArea.childCount - 1, newBox);
            }
        }
 
        void OnDragExitedEvent(DragExitedEvent e)
        {
            // Never called at the moment due to a bug. Listen to DragLeaveEvent instead.
            Debug.Log("Should not be called unless bug was fixed.");
        }
    }
}
 
