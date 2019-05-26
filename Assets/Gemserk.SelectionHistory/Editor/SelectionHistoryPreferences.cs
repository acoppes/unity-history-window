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

	    private static bool drawFavorites = true;

	    // [PreferenceItem("Selection History")]
		private static void PreferencesGUI()
		{
			if (!prefsLoaded)
			{
				historySize = EditorPrefs.GetInt (SelectionHistoryWindowConstants.HistorySizePrefKey, 10);
				autoremoveDeleted = EditorPrefs.GetBool(SelectionHistoryWindowConstants.HistoryAutomaticRemoveDeletedPrefKey, true);
				autoRemoveDuplicated = EditorPrefs.GetBool(SelectionHistoryWindowConstants.HistoryAllowDuplicatedEntriesPrefKey, false);
			    showHierarchyObjects = EditorPrefs.GetBool(SelectionHistoryWindowConstants.HistoryShowHierarchyObjectsPrefKey, true);
			    showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindowConstants.HistoryShowProjectViewObjectsPrefKey, true);
			    drawFavorites = EditorPrefs.GetBool(SelectionHistoryWindowConstants.HistoryFavoritesPrefKey, true);
                prefsLoaded = true;
			}

			historySize = EditorGUILayout.IntField("History Size", historySize);
			autoremoveDeleted = EditorGUILayout.Toggle ("Auto Remove Deleted", autoremoveDeleted);
			autoRemoveDuplicated = EditorGUILayout.Toggle ("Allow duplicated entries", autoRemoveDuplicated);
		    showHierarchyObjects = EditorGUILayout.Toggle("Show HierarchyView objects", showHierarchyObjects);
		    showProjectViewObjects = EditorGUILayout.Toggle("Show ProjectView objects", showProjectViewObjects);
		    drawFavorites = EditorGUILayout.Toggle("Favorites Enabled", drawFavorites);

            if (GUI.changed) {
				EditorPrefs.SetInt(SelectionHistoryWindowConstants.HistorySizePrefKey, historySize);
				EditorPrefs.SetBool(SelectionHistoryWindowConstants.HistoryAutomaticRemoveDeletedPrefKey, autoremoveDeleted);
				EditorPrefs.SetBool(SelectionHistoryWindowConstants.HistoryAllowDuplicatedEntriesPrefKey, autoRemoveDuplicated);
                
                EditorPrefs.SetBool(SelectionHistoryWindowConstants.HistoryShowHierarchyObjectsPrefKey, showHierarchyObjects);
                EditorPrefs.SetBool(SelectionHistoryWindowConstants.HistoryShowProjectViewObjectsPrefKey, showProjectViewObjects);
                EditorPrefs.SetBool(SelectionHistoryWindowConstants.HistoryFavoritesPrefKey, drawFavorites);

                SelectionHistoryWindow.shouldReloadPreferences = true;
			}
		}

		[SettingsProvider]
		public static SettingsProvider CreateMyCustom()
		{
			var provider = new SettingsProvider("Gemserk/Selection History", SettingsScope.User)
			{
				label = "Selection History",
				guiHandler = (searchContext) =>
				{
					// var settings = MyCustomSettings.GetSerializedSettings();
					// int number = 5;
					// EditorGUILayout.IntField(new GUIContent("My Number"), number);
					PreferencesGUI();
				},
				// keywords = new []{ "My Number" }
			};
			
			return provider;
		}
	}
}
