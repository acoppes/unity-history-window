using UnityEngine;
using System.Collections;

using UnityEditor;

[CustomEditor (typeof(TriggerContainer))]
public class TriggerContainerEditor : Editor
{

	private SerializedObject obj;

	public void OnEnable ()
	{
		obj = new SerializedObject (target);
	}

	public override void OnInspectorGUI ()
	{
		DrawDefaultInspector ();
		EditorGUILayout.Space ();
//		DropAreaGUI ();
	}

//	public void DropAreaGUI ()
//	{
//		Event evt = Event.current;
//		Rect drop_area = GUILayoutUtility.GetRect (0.0f, 50.0f, GUILayout.ExpandWidth (true));
//		GUI.Box (drop_area, "Add Trigger");
//
//		switch (evt.type) {
//		case EventType.DragUpdated:
//		case EventType.DragPerform:
//			if (!drop_area.Contains (evt.mousePosition))
//				return;
//
//			DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
//
//			if (evt.type == EventType.DragPerform) {
//				DragAndDrop.AcceptDrag ();
//
//				foreach (Object dragged_object in DragAndDrop.objectReferences) {
//					// Do On Drag Stuff here
//				}
//			}
//			break;
//		}
//	}
}