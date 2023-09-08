using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gemserk
{
    [InitializeOnLoad]
    public static class StoreSceneSelection
    {
        static StoreSceneSelection()
        {
            EditorSceneManager.sceneOpened += StoreSceneSelectionOnSceneOpened;
        }

        public static void RestoreSceneReferences()
        {
            var selectionHistory = SelectionHistoryReference.SelectionHistory;

            if (selectionHistory == null)
                return;
			
            var entries = selectionHistory.History;

            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.globalObjectId))
                {
                    // This only parses the global id but that doesnt mean its object is not null
                    if (GlobalObjectId.TryParse(entry.globalObjectId, out var globalObjectId))
                    {
                        var reference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                        if (reference != null)
                        {
                            // Debug.Log($"Restoring scene object Reference {entry.name} from GlobalId");
                            entry.hierarchyObjectReference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                        }
                    }
                }
            }
        }

        private static void StoreSceneSelectionOnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            RestoreSceneReferences();
        }
    }
}