using UnityEngine;

namespace Gemserk
{
    public class SelectionHistoryAsset : ScriptableObject
    {
        public SelectionHistory selectionHistory = new SelectionHistory();

#if UNITY_EDITOR
        private void OnEnable()
        {
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded += OnNewEntryAdded;
            }
        }
        
        private void OnDisable()
        {
            if (selectionHistory != null)
            {
                selectionHistory.OnNewEntryAdded -= OnNewEntryAdded;
            }
        }

        private void OnNewEntryAdded(SelectionHistory obj)
        {
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}