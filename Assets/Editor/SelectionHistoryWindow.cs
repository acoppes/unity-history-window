using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SelectionHistoryWindow : EditorWindow {

	readonly List<Object> history = new List<Object>(100);

	int currentSelectionIndex;

	Object currentSelection;

	int historySize = 10;

	// Add menu named "My Window" to the Window menu
	[MenuItem ("Window/Gemserk/Selection History")]
	static void Init () {
		// Get existing open window or if none, make a new one:
		SelectionHistoryWindow window = EditorWindow.GetWindow<SelectionHistoryWindow>("Selection History");
		window.Show();
	}

	void OnEnable()
	{
		//		history.Clear();

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

	Vector2 scrollPosition;

	void OnGUI () {

		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

		historySize = EditorGUILayout.IntField("History Size", historySize);

		DrawHistory();

		if (GUILayout.Button("Previous")) {
			Previous();
			Selection.activeObject = GetSelection();
		}

		if (GUILayout.Button("Next")) {
			Next();
			Selection.activeObject = GetSelection();
		}

		if (GUILayout.Button("Clear")) {
			history.Clear();
			Repaint();
		}

		EditorGUILayout.EndScrollView();

	}

	void DrawHistory()
	{
		var nonSelectedColor = GUI.color;

		EditorGUI.BeginDisabledGroup(true);
		for (int i = 0; i < history.Count; i++) {
			var historyElement = history [i];

			if (currentSelectionIndex == i) {
				GUI.color = Color.green;
			} else {
				GUI.color = nonSelectedColor;
			}

			if (historyElement == null) {
				EditorGUILayout.LabelField("Deleted");
				continue;
			}

			EditorGUILayout.ObjectField(historyElement, historyElement.GetType(), true);
		}
		EditorGUI.EndDisabledGroup();

		GUI.color = nonSelectedColor;
	}

}
