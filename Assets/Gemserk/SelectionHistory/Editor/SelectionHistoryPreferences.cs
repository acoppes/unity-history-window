using UnityEngine;
using UnityEditor;

namespace Gemserk
{
	public static class SelectionHistoryPreferences {

		static bool prefsLoaded = false;

		static int historySize;

		static bool autoremoveDeleted;

		static bool autoRemoveDuplicated;

		[PreferenceItem("Selection History")]
		public static void PreferencesGUI()
		{
			if (!prefsLoaded)
			{
				historySize = EditorPrefs.GetInt (SelectionHistoryWindow.HistorySizePrefKey, 10);
				autoremoveDeleted = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
				autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);
				prefsLoaded = true;
			}

			historySize = EditorGUILayout.IntField("History Size", historySize);
			autoremoveDeleted = EditorGUILayout.Toggle ("Auto Remove Deleted", autoremoveDeleted);
			autoRemoveDuplicated = EditorGUILayout.Toggle ("Allow duplicated entries", autoRemoveDuplicated);

			if (GUI.changed) {
				EditorPrefs.SetInt(SelectionHistoryWindow.HistorySizePrefKey, historySize);
				EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDeleted);
				EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);

				SelectionHistoryWindow.shouldReloadPreferences = true;
			}
		}
	}
}
