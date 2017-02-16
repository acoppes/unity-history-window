using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Gemserk
{
	[InitializeOnLoad]
	public static class SelectionHistoryInitialized
	{
		static SelectionHistoryInitialized()
		{
			SelectionHistoryWindow.RegisterSelectionListener ();
		}
	}

	public class SelectionHistoryWindow : EditorWindow {

		static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";
		static readonly string HistoryBackgroundEnabledPrefKey = "Gemserk.SelectionHistory.RunInBackgroundEnabled";

		static readonly SelectionHistory selectionHistory = new SelectionHistory();

		static bool debugEnabled = false;



		// Add menu named "My Window" to the Window menu
		[MenuItem ("Window/Gemserk/Selection History %#h")]
		static void Init () {
			// Get existing open window or if none, make a new one:
//			var window = ScriptableObject.CreateInstance<SelectionHistoryWindow>();
			var window = EditorWindow.GetWindow<SelectionHistoryWindow> ();

			window.titleContent.text = "History";
			window.Show();
		}

		static void SelectionRecorder ()
		{
			if (Selection.activeObject != null) {
				if (debugEnabled) {
					Debug.Log ("Recording new selection: " + Selection.activeObject.name);
				}

				selectionHistory.History = EditorTemporaryMemory.Instance.history;
				selectionHistory.UpdateSelection (Selection.activeObject);
			}
		}

		public static void RegisterSelectionListener()
		{
			if (!EditorPrefs.GetBool (HistoryBackgroundEnabledPrefKey, false))
				return;
			Selection.selectionChanged += SelectionRecorder;
		}
	
		public static void UnregisterSelectionListener()
		{
			Selection.selectionChanged -= SelectionRecorder;
		}

		public GUISkin windowSkin;

		void OnEnable()
		{
			selectionHistory.History = EditorTemporaryMemory.Instance.history;

			selectionHistory.HistorySize = EditorPrefs.GetInt (HistorySizePrefKey, 10);

			Selection.selectionChanged += delegate {
				Repaint();
			};
		}

		void Update()
		{
			if (Selection.activeObject == null)
				return;
			selectionHistory.UpdateSelection (Selection.activeGameObject);
		}

		void UpdateSelection(int currentIndex)
		{
			Selection.activeObject = selectionHistory.UpdateSelection(currentIndex);
		}

		Vector2 scrollPosition;

		void OnGUI () {

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			int currentHistorySize = selectionHistory.HistorySize;

			selectionHistory.HistorySize = EditorGUILayout.IntField("History Size", selectionHistory.HistorySize);

			if (selectionHistory.HistorySize != currentHistorySize) {
				// updates user pref for history size
				EditorPrefs.SetInt(HistorySizePrefKey, selectionHistory.HistorySize);
			}

			var runInBackgroundEnabled = EditorPrefs.GetBool (HistoryBackgroundEnabledPrefKey, false);
			var newRunInBackground = GUILayout.Toggle (runInBackgroundEnabled, "Run in background");

			if (runInBackgroundEnabled && !newRunInBackground) {
				EditorPrefs.SetBool (HistoryBackgroundEnabledPrefKey, false);
				UnregisterSelectionListener ();
			} else if (!runInBackgroundEnabled && newRunInBackground) {
				EditorPrefs.SetBool (HistoryBackgroundEnabledPrefKey, true);
				RegisterSelectionListener ();
			}

			DrawHistory();

			if (GUILayout.Button("Previous", windowSkin.button)) {
				selectionHistory.Previous();
				Selection.activeObject = selectionHistory.GetSelection();
			}

			if (GUILayout.Button("Next", windowSkin.button)) {
				selectionHistory.Next();
				Selection.activeObject = selectionHistory.GetSelection();
			}

			if (GUILayout.Button("Clear", windowSkin.button)) {
				selectionHistory.Clear();
				Repaint();
			}

			EditorGUILayout.EndScrollView();

		}

		void DrawHistory()
		{
			var nonSelectedColor = GUI.backgroundColor;

			var history = selectionHistory.History;

			for (int i = 0; i < history.Count; i++) {
				var historyElement = history [i];

				if (selectionHistory.IsSelected(i)) {
					GUI.backgroundColor = Color.cyan;
				} else {
					GUI.backgroundColor = nonSelectedColor;
				}

				if (historyElement == null) {
					EditorGUILayout.LabelField("Deleted");
					continue;
				}
					
				EditorGUILayout.BeginHorizontal ();

				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.ObjectField(historyElement, historyElement.GetType(), true);
				EditorGUI.EndDisabledGroup();

				GUI.backgroundColor = nonSelectedColor;

				var buttonStyle = windowSkin.GetStyle("SelectionButton");

				if (GUILayout.Button ("Select", buttonStyle)) {
					UpdateSelection (i);
				}

				EditorGUILayout.EndHorizontal ();
			}

			GUI.backgroundColor = nonSelectedColor;
		}

	}
}