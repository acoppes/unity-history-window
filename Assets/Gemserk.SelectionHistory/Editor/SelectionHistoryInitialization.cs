using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Gemserk
{
    [InitializeOnLoad]
    public static class SelectionHistoryInitialization
    {
        private static readonly bool debugEnabled = false;
		
        static SelectionHistoryInitialization()
        {
            Selection.selectionChanged += SelectionRecorder;
        }
		
        private static void SelectionRecorder ()
        {
            if (Selection.activeObject != null) {
                if (debugEnabled) {
                    Debug.Log ("Recording new selection: " + Selection.activeObject.name);
                }

                var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
                selectionHistory.UpdateSelection (Selection.activeObject);
            } 
        }
		
        [MenuItem("Window/Gemserk/Previous selection %#,")]
        [Shortcut("Selection History/Previous Selection")]
        public static void PreviousSelection()
        {
            var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
            selectionHistory.Previous ();
            Selection.activeObject = selectionHistory.GetSelection ();
        }

        [MenuItem("Window/Gemserk/Next selection %#.")]
        [Shortcut("Selection History/Next Selection")]
        public static void NextSelection()
        {
            var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
            selectionHistory.Next();
            Selection.activeObject = selectionHistory.GetSelection ();
        }
    }
}