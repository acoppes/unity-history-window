using UnityEngine;
using UnityEditor;

namespace Gemserk {
    public static class SelectionHistoryPreferences {

        static bool prefsLoaded = false;

       // static int historySize;

        static bool autoremoveDestroyed;

        static bool autoRemoveDuplicated;

        private static bool showProjectViewObjects = true;

        private static bool drawFavorites = true;

        private static bool orderLastSelectedFirst = true;

        private static bool backgroundRecord;

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) =>
                {
                    var selectionHistory = SelectionHistoryReference.SelectionHistory;
                    
                    if (!prefsLoaded) {
                       // historySize = EditorPrefs.GetInt(SelectionHistoryWindowUtils.HistorySizePrefKey, defaultHistorySize);
                        autoremoveDestroyed = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDeletedPrefKey, true);
                        autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, false);
                        showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowProjectViewObjectsPrefKey, true);
                        drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, true);
                        orderLastSelectedFirst = EditorPrefs.GetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, false);
                        backgroundRecord = EditorPrefs.GetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, false);
                        prefsLoaded = true;
                    }
                    
                    if (selectionHistory != null)
                    {
                        selectionHistory.historySize =
                            EditorGUILayout.IntField("History Size", selectionHistory.historySize);
                    }
                    
                    autoremoveDestroyed = EditorGUILayout.Toggle("Auto Remove Destroyed", autoremoveDestroyed);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("Allow duplicated entries", autoRemoveDuplicated);
                    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);
                    drawFavorites = EditorGUILayout.Toggle("Show Pin to favorites button", drawFavorites);
                    orderLastSelectedFirst = EditorGUILayout.Toggle("Order last selected first", orderLastSelectedFirst);
                    backgroundRecord = EditorGUILayout.Toggle("Record selection while window closed", backgroundRecord);

                    if (GUI.changed) {
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDestroyed);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);

                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, drawFavorites);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, orderLastSelectedFirst);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, backgroundRecord);

                        // var window = EditorWindow.GetWindow<SelectionHistoryWindow>();
                        // if (window != null)
                        // {
                        //     window.ReloadRootAndRemoveUnloadedAndDuplicated();
                        // }

                        if (SelectionHistoryReference.asset != null)
                        {
                            EditorUtility.SetDirty(SelectionHistoryReference.asset);
                        }
                    }
                },

            };
            return provider;
        }
    }
}
