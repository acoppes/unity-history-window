using UnityEngine;
using UnityEditor;

namespace Gemserk {
    
    [InitializeOnLoad]
    public static class SelectionHistoryPreferences {

        static bool prefsLoaded = false;

       // static int historySize;

        static bool autoremoveDestroyed;
        static bool autoremoveUnloaded;

        static bool autoRemoveDuplicated;

        private static bool drawFavorites = true;

        private static bool orderLastSelectedFirst = true;

        private static bool backgroundRecord;

        public static bool nativeKeyHandleDisabled;
        
        static SelectionHistoryPreferences()
        {
            // historySize = EditorPrefs.GetInt(SelectionHistoryWindowUtils.HistorySizePrefKey, defaultHistorySize);
            autoremoveDestroyed = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDestroyedPrefKey, true);
            autoremoveUnloaded = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveUnloadedPrefKey, true);
            autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, false);
            drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, true);
            orderLastSelectedFirst = EditorPrefs.GetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, false);
            backgroundRecord = EditorPrefs.GetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, false);
            nativeKeyHandleDisabled = EditorPrefs.GetBool(SelectionHistoryWindowUtils.NativeKeyHandleDisabledKey, false);
            prefsLoaded = true;
        }

        [SettingsProvider]
        public static SettingsProvider CreateSelectionHistorySettingsProvider() {
            var provider = new SettingsProvider("Selection History", SettingsScope.User) {
                label = "Selection History",
                guiHandler = (searchContext) =>
                {
                    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
                    
                    if (selectionHistory != null)
                    {
                        selectionHistory.historySize =
                            EditorGUILayout.IntField("History Size", selectionHistory.historySize);
                    }
                    
                    autoremoveDestroyed = EditorGUILayout.Toggle("Auto Remove Destroyed", autoremoveDestroyed);
                    autoremoveUnloaded = EditorGUILayout.Toggle("Auto Remove Unloaded", autoremoveUnloaded);
                    autoRemoveDuplicated = EditorGUILayout.Toggle("Allow duplicated entries", autoRemoveDuplicated);
                    drawFavorites = EditorGUILayout.Toggle("Show Pin to favorites button", drawFavorites);
                    orderLastSelectedFirst = EditorGUILayout.Toggle("Order last selected first", orderLastSelectedFirst);
                    backgroundRecord = EditorGUILayout.Toggle("Record selection while window closed", backgroundRecord);
                    nativeKeyHandleDisabled = EditorGUILayout.Toggle("Disable native mouse handle (button 4 & 5)", nativeKeyHandleDisabled);

                    if (GUI.changed) {
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveDestroyedPrefKey, autoremoveDestroyed);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAutomaticRemoveUnloadedPrefKey, autoremoveUnloaded);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.HistoryShowPinButtonPrefKey, drawFavorites);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.OrderLastSelectedFirstKey, orderLastSelectedFirst);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.BackgroundRecordKey, backgroundRecord);
                        EditorPrefs.SetBool(SelectionHistoryWindowUtils.NativeKeyHandleDisabledKey, nativeKeyHandleDisabled);

                        // var window = EditorWindow.GetWindow<SelectionHistoryWindow>();
                        // if (window != null)
                        // {
                        //     window.ReloadRootAndRemoveUnloadedAndDuplicated();
                        // }

                        SelectionHistoryAsset.instance.ForceSave();
                    }
                },

            };
            return provider;
        }
    }
}
