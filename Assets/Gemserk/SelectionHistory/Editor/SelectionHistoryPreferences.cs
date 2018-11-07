using UnityEngine;
using UnityEditor;

namespace Gemserk
{
	public static class SelectionHistoryPreferences {

		static bool prefsLoaded = false;

		static int historySize;

		static bool autoremoveDeleted;

		static bool autoRemoveDuplicated;

	    private static bool showHierarchyObjects = true;
	    private static bool showProjectViewObjects = true;

        [PreferenceItem("Selection History")]
		public static void PreferencesGUI()
		{
			if (!prefsLoaded)
			{
				historySize = EditorPrefs.GetInt (SelectionHistoryWindow.HistorySizePrefKey, 10);
				autoremoveDeleted = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
				autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);
			    showHierarchyObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, true);
			    showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, true);
                prefsLoaded = true;
			}

			historySize = EditorGUILayout.IntField("History Size", historySize);
			autoremoveDeleted = EditorGUILayout.Toggle ("Auto Remove Deleted", autoremoveDeleted);
			autoRemoveDuplicated = EditorGUILayout.Toggle ("Allow duplicated entries", autoRemoveDuplicated);
		    showHierarchyObjects = EditorGUILayout.Toggle("Show HierarchyView objects", showHierarchyObjects);
		    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);

            if (GUI.changed) {
				EditorPrefs.SetInt(SelectionHistoryWindow.HistorySizePrefKey, historySize);
				EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDeleted);
				EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);
                
                EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, showHierarchyObjects);
                EditorPrefs.SetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);

                SelectionHistoryWindow.shouldReloadPreferences = true;
			}
		}
	}
}
