using UnityEngine;
using UnityEditor;

namespace Gemserk
{
	public static class SelectionHistoryPreferences {

		static bool prefsLoaded = false;

		static int historySize;

		static bool autoremoveDeleted;

		[PreferenceItem("Selection History")]

		public static void PreferencesGUI()
		{
			if (!prefsLoaded)
			{
				historySize = EditorPrefs.GetInt (SelectionHistoryWindow.HistorySizePrefKey, 10);
				autoremoveDeleted = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
				prefsLoaded = true;
			}

			historySize = EditorGUILayout.IntField("History Size", historySize);
			autoremoveDeleted = EditorGUILayout.Toggle ("Auto Remove Deleted", autoremoveDeleted);

			if (GUI.changed) {
				EditorPrefs.SetInt(SelectionHistoryWindow.HistorySizePrefKey, historySize);
				EditorPrefs.SetBool(SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDeleted);

				SelectionHistoryWindow.shouldReloadPreferences = true;
			}
		}
	}
}
