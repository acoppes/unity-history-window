using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    [FilePath("Gemserk/Gemserk.SelectionHistory.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SelectionHistoryAsset : ScriptableSingleton<SelectionHistoryAsset>
    {
        [SerializeField]
        public SelectionHistory selectionHistory = new SelectionHistory();
        
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
            // EditorUtility.SetDirty(this);
            Save(true);
            // Debug.Log("Saved to: " + GetFilePath());
        }

        public void ForceSave()
        {
            Save(true);
        }
    }
}