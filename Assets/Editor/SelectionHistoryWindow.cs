using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace Gemserk
{
	public class SelectionHistoryWindow : EditorWindow {

		static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";

		readonly List<Object> history = new List<Object>(100);

		int currentSelectionIndex;

		Object currentSelection;

		int historySize = 10;

		public GUISkin windowSkin;

		// Add menu named "My Window" to the Window menu
		[MenuItem ("Window/Gemserk/Selection History")]
		static void Init () {
			// Get existing open window or if none, make a new one:
			SelectionHistoryWindow window = EditorWindow.GetWindow<SelectionHistoryWindow>("History");
			window.Show();
		}

		void OnEnable()
		{
			historySize = EditorPrefs.GetInt (HistorySizePrefKey, 10);

//			windowSkin = AssetDatabase.LoadAssetAtPath<GUISkin> ("Editor/SelectionHistorySkin");

			Selection.selectionChanged += delegate {
				Repaint();
			};
		}

		void Update()
		{
			if (Selection.activeObject == null)
				return;
			UpdateSelection(Selection.activeObject);
		}

		public int GetHistoryCount()
		{
			return history.Count;	
		}

		public Object GetSelection()
		{
			return currentSelection;
		}

		public void UpdateSelection(Object selection)
		{
			var lastSelectedObject = history.Count > 0 ? history.Last() : null;

			if (lastSelectedObject != selection && currentSelection != selection) {
				history.Add(selection);
				currentSelectionIndex = history.Count - 1;
			}

			currentSelection = selection;

			if (history.Count > historySize) {
				history.RemoveRange(0, history.Count - historySize);
				//			history.RemoveAt(0);
			}
		}

		public void Previous()
		{
			if (history.Count == 0)
				return;

			currentSelectionIndex--;
			if (currentSelectionIndex < 0)
				currentSelectionIndex = 0;
			currentSelection = history[currentSelectionIndex];
		}

		public void Next()
		{
			if (history.Count == 0)
				return;

			currentSelectionIndex++;
			if (currentSelectionIndex >= history.Count)
				currentSelectionIndex = history.Count - 1;
			currentSelection = history[currentSelectionIndex];
		}

		void UpdateSelection(int currentIndex)
		{
			currentSelectionIndex = currentIndex;
			currentSelection = history[currentSelectionIndex];

			Selection.activeObject = currentSelection;
		}

		Vector2 scrollPosition;

		void OnGUI () {

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			int currentHistorySize = historySize;

			historySize = EditorGUILayout.IntField("History Size", historySize);

			if (historySize != currentHistorySize) {
				// updates user pref for history size
				EditorPrefs.SetInt(HistorySizePrefKey, historySize);
			}

			DrawHistory();

			if (GUILayout.Button("Previous", windowSkin.button)) {
				Previous();
				Selection.activeObject = GetSelection();
			}

			if (GUILayout.Button("Next", windowSkin.button)) {
				Next();
				Selection.activeObject = GetSelection();
			}

			if (GUILayout.Button("Clear", windowSkin.button)) {
				history.Clear();
				Repaint();
			}

			EditorGUILayout.EndScrollView();

		}

		void DrawHistory()
		{
			var nonSelectedColor = GUI.backgroundColor;

			for (int i = 0; i < history.Count; i++) {
				var historyElement = history [i];

				if (currentSelectionIndex == i) {
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