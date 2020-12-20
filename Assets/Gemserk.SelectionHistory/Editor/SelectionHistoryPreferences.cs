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

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) => {
                    if (!prefsLoaded) {
                        historySize = EditorPrefs.GetInt(SelectionHistoryWindow.HistorySizePrefKey, 10);
                        autoremoveDeleted = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
                        autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);
                        showHierarchyObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, true);
                        showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, true);
                        drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryFavoritesPrefKey, true);
                        prefsLoaded = true;
                    }

                    historySize = EditorGUILayout.IntField("History Size", historySize);
                    autoremoveDeleted = EditorGUILayout.Toggle("Auto Remove Deleted", autoremoveDeleted);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("Allow duplicated entries", autoRemoveDuplicated);
                    showHierarchyObjects = EditorGUILayout.Toggle("Show HierarchyView objects", showHierarchyObjects);
                    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);
                    drawFavorites = EditorGUILayout.Toggle("Favorites Enabled", drawFavorites);

                    if (GUI.changed) {
                        EditorPrefs.SetInt(SelectionHistoryWindow.HistorySizePrefKey, historySize);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDeleted);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);

                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, showHierarchyObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryFavoritesPrefKey, drawFavorites);

                        SelectionHistoryWindow.shouldReloadPreferences = true;
                    }
                },

            };
            return provider;
        }
    }
}
