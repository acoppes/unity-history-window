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

        private const int defaultHistorySize = 500;
        
        private static bool orderLastSelectedFirst = true;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) =>
                {
                    if (!prefsLoaded) {
                        historySize = EditorPrefs.GetInt(SelectionHistoryWindowUtils.HistorySizePrefKey, defaultHistorySize);
                        autoremoveDestroyed = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDeletedPrefKey, true);
                        autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, false);
                        showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowProjectViewObjectsPrefKey, true);
                        drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, true);
                        orderLastSelectedFirst = EditorPrefs.GetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, false);
                        prefsLoaded = true;
                    }

                    historySize = EditorGUILayout.IntField("History Size", historySize);
                    autoremoveDestroyed = EditorGUILayout.Toggle("Auto Remove Destroyed", autoremoveDestroyed);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("Allow duplicated entries", autoRemoveDuplicated);
                    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);
                    drawFavorites = EditorGUILayout.Toggle("Show Pin to favorites button", drawFavorites);
                    orderLastSelectedFirst = EditorGUILayout.Toggle("Order last selected first", orderLastSelectedFirst);

                    if (GUI.changed) {
                        EditorPrefs.SetInt(SelectionHistoryWindowUtils.HistorySizePrefKey, historySize);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDestroyed);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);

                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, drawFavorites);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, orderLastSelectedFirst);

                        var window = EditorWindow.GetWindow<SelectionHistoryWindow>();
                        if (window != null)
                        {
                            window.ReloadRoot();
                        }
                    }
                },

            };
            return provider;
        }
    }
}
