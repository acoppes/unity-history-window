using UnityEngine;
using UnityEditor;

namespace Gemserk
{
	[InitializeOnLoad]
	public class SelectionHistoryRecorder {

		static SelectionHistoryRecorder()
		{
			Selection.selectionChanged += delegate() {
				if (Selection.activeObject != null) {
					Debug.Log ("Selection:" + Selection.activeObject.name);
					SelectionHistoryWindow.selectionHistory.UpdateSelection(Selection.activeObject);
					//					SelectionHistoryWindow.storedHistory.Add(Selection.activeObject);
				}
			};
			Debug.Log("Up and running");
		}

	}
}