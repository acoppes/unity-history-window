using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    [InitializeOnLoad]
    public static class SelectionHistoryContext
    {
        private static readonly bool debugEnabled = false;
		
        private static SelectionHistory _selectionHistory = new SelectionHistory();

        public static SelectionHistory SelectionHistory
        {
            get
            {
                _selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
                return _selectionHistory;
            }
        }

        static SelectionHistoryContext()
        {
            // Init();
            RegisterSelectionListener();
        }

//		private static void Init()
//		{
//			_selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
//		}
		
        private static void SelectionRecorder ()
        {
            if (Selection.activeObject == null) 
                return;
			
            if (debugEnabled) {
                Debug.LogFormat("Recording new selection: {0}", Selection.activeObject.name);
            }

            _selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
            _selectionHistory.UpdateSelection (Selection.activeObject);
        }

        private static void RegisterSelectionListener()
        {
            Selection.selectionChanged += SelectionRecorder;
        }
    }
}