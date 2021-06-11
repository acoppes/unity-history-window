using UnityEngine;
using UnityEditor;

namespace Gemserk {
    public static class SelectionHistoryPreferences {

        static bool prefsLoaded = false;

        static int historySize;

        static bool autoremoveDestroyed;

        static bool autoRemoveDuplicated;

        private static bool showProjectViewObjects = true;

        private static bool drawFavorites = true;
        
        private static bool showDestroyedObjects = false;

        private const int defaultHistorySize = 500;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) => {
                    if (!prefsLoaded) {
                        historySize = EditorPrefs.GetInt(SelectionHistoryWindow.HistorySizePrefKey, defaultHistorySize);
                        autoremoveDestroyed = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
                        autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);
                        showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, true);
                        drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowPinButtonPrefKey, true);
                        showDestroyedObjects = EditorPrefs.GetBool(SelectionHistoryWindow.ShowDestroyedObjectsKey, false);

                        prefsLoaded = true;
                    }

                    historySize = EditorGUILayout.IntField("History Size", historySize);
                    autoremoveDestroyed = EditorGUILayout.Toggle("Auto Remove Destroyed", autoremoveDestroyed);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("Allow duplicated entries", autoRemoveDuplicated);
                    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);
                    drawFavorites = EditorGUILayout.Toggle("Show Pin to favorites button", drawFavorites);

                    showDestroyedObjects = EditorGUILayout.Toggle(new GUIContent()
                    {
                        text = "Show Destroyed Objects",
                        tooltip = "Toggle to show/hide unreferenced or destroyed objects."
                    }, showDestroyedObjects);

                    if (GUI.changed) {
                        EditorPrefs.SetInt(SelectionHistoryWindow.HistorySizePrefKey, historySize);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDestroyed);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);

                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowPinButtonPrefKey, drawFavorites);
                        
                        EditorPrefs.SetBool(SelectionHistoryWindow.ShowDestroyedObjectsKey, showDestroyedObjects);

                        SelectionHistoryWindow.shouldReloadPreferences = true;
                    }
                },

            };
            return provider;
        }
    }
}
