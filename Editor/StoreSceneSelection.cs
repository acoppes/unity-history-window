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
            EditorSceneManager.sceneClosing += StoreSceneSelectionOnSceneClosing;
            EditorSceneManager.sceneOpened += StoreSceneSelectionOnSceneOpened;
        }

        private static void StoreSceneSelectionOnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;

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
                            // Debug.Log($"Restoring scene object reference {entry.name} from GlobalId");
                            entry.reference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
                            entry.globalObjectId = null;
                        }
                    }
                }
            }
        }

        private static void StoreSceneSelectionOnSceneClosing(Scene scene, bool removingScene)
        {
            if (!removingScene)
                return;
			
            var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;

            if (selectionHistory == null)
                return;
			
            var entries = selectionHistory.History;
            foreach (var entry in entries)
            {
                if (entry.reference != null && entry.reference is GameObject go)
                {
                    // GameObject's scene is being unloaded here...
                    if (go.scene == scene)
                    {
                        entry.globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
                        // var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                        // AssetDatabase.GetAssetPath(scene);
                        // Debug.Log($"Storing scene object reference {entry.name} as GlobalId");
                        // entry.state = SelectionHistory.Entry.State.ReferenceUnloaded;
                    }
                }
            }
        }
    }
}