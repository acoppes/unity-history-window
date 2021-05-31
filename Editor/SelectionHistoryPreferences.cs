using UnityEngine;
using UnityEditor;

namespace Gemserk {
    public static class SelectionHistoryPreferences {

        static bool prefsLoaded = false;

        static int historySize;

        static bool autoremoveDeleted;

        static bool autoRemoveDuplicated;

        private static bool showHierarchyObjects = true;
        private static bool showProjectViewObjects = true;

        private static bool drawFavorites = true;
        
        private static bool showUnloadedObjects = true;
        private static bool showDestroyedObjects = false;

        private const int defaultHistorySize = 500;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) => {
                    if (!prefsLoaded) {
                        historySize = EditorPrefs.GetInt(SelectionHistoryWindow.HistorySizePrefKey, defaultHistorySize);
                        autoremoveDeleted = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
                        autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);
                        showHierarchyObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, true);
                        showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, true);
                        drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryFavoritesPrefKey, true);

                        showUnloadedObjects = EditorPrefs.GetBool(SelectionHistoryWindow.ShowUnloadedObjectsKey, true);
                        showDestroyedObjects = EditorPrefs.GetBool(SelectionHistoryWindow.ShowDestroyedObjectsKey, false);

                        prefsLoaded = true;
                    }

                    historySize = EditorGUILayout.IntField("History Size", historySize);
                    autoremoveDeleted = EditorGUILayout.Toggle("Auto Remove Unreferenced", autoremoveDeleted);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("Allow duplicated entries", autoRemoveDuplicated);
                    showHierarchyObjects = EditorGUILayout.Toggle("Show HierarchyView objects", showHierarchyObjects);
                    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);
                    drawFavorites = EditorGUILayout.Toggle("Favorites Enabled", drawFavorites);

                    showUnloadedObjects = EditorGUILayout.Toggle(new GUIContent()
                    {
                        text = "Show Unloaded Objects",
                        tooltip = "Toggle to show/hide objects from unloaded scenes."
                    }, showUnloadedObjects);
                    
                    showDestroyedObjects = EditorGUILayout.Toggle(new GUIContent()
                    {
                        text = "Show Destroyed Objects",
                        tooltip = "Toggle to show/hide unreferenced or destroyed objects."
                    }, showDestroyedObjects);

                    if (GUI.changed) {
                        EditorPrefs.SetInt(SelectionHistoryWindow.HistorySizePrefKey, historySize);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDeleted);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);

                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, showHierarchyObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryFavoritesPrefKey, drawFavorites);
                        
                        EditorPrefs.SetBool(SelectionHistoryWindow.ShowUnloadedObjectsKey, showUnloadedObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindow.ShowDestroyedObjectsKey, showDestroyedObjects);

                        SelectionHistoryWindow.shouldReloadPreferences = true;
                    }
                },

            };
            return provider;
        }
    }
}
