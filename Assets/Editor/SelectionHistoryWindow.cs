using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Gemserk
{
	public class SelectionHistoryWindow : EditorWindow {

		static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";

		public static SelectionHistory selectionHistory = new SelectionHistory();

		// Add menu named "My Window" to the Window menu
		[MenuItem ("Window/Gemserk/Selection History %#h")]
		static void Init () {
			// Get existing open window or if none, make a new one:
			var window = ScriptableObject.CreateInstance<SelectionHistoryWindow>();

//			SelectionHistoryWindow window = EditorWindow.GetWindow<SelectionHistoryWindow>("History");
			window.titleContent.text = "History";
//			window.History = storedHistory;
			window.Show();
		}
	
		public GUISkin windowSkin;

		void OnEnable()
		{
			selectionHistory.HistorySize = EditorPrefs.GetInt (HistorySizePrefKey, 10);

//			windowSkin = AssetDatabase.LoadAssetAtPath<GUISkin> ("Editor/SelectionHistorySkin");

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