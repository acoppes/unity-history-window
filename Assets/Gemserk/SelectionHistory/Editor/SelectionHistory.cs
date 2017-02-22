using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SelectionHistory  {

	List<Object> history = new List<Object>(100);

	int currentSelectionIndex;

	Object currentSelection;

	int historySize = 10;

	public List<Object> History {
		get {
			return history;
		}
		set {
			history = value;
		}
	}

	public int HistorySize {
		get {
			return historySize;
		}
		set {
			historySize = value;
		}
	}

	public bool IsSelected(int index)
	{
		return index == currentSelectionIndex;
	}

	public void Clear()
	{
		history.Clear ();
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
		if (selection == null)
			return;
		
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

	public Object UpdateSelection(int currentIndex)
	{
		currentSelectionIndex = currentIndex;
		currentSelection = history[currentSelectionIndex];

		return currentSelection;
	}

	public void ClearDeleted()
	{
		var deletedCount = history.Count(e => e == null);

		history.RemoveAll (e => e == null);

		currentSelectionIndex -= deletedCount;

		if (currentSelectionIndex < 0)
			currentSelectionIndex = 0;

		if (currentSelection == null)
			currentSelectionIndex = -1;
	}

	public void RemoveDuplicated()
	{
		var tempList = new List<Object> (history);

		foreach (var item in tempList) {
			var itemFirstIndex = history.IndexOf (item);
			var itemLastIndex = history.LastIndexOf (item);

			while (itemFirstIndex != itemLastIndex) {
				history.RemoveAt (itemFirstIndex);

				itemFirstIndex = history.IndexOf (item);
				itemLastIndex = history.LastIndexOf (item);
			}
		}

		if (currentSelectionIndex >= history.Count)
			currentSelectionIndex = history.Count - 1;
	}

}
