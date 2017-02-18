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

	public class SelectionRect
	{
		public Rect rect;
		public Object element;
	}

	public class SelectionHistoryWindow : EditorWindow {

		static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";
		static readonly string HistoryBackgroundEnabledPrefKey = "Gemserk.SelectionHistory.RunInBackgroundEnabled";

		static readonly SelectionHistory selectionHistory = new SelectionHistory();

		static readonly bool debugEnabled = false;

		static readonly bool prevNextButtonsEnabled = false;

		static readonly bool runInBackgroundConfigEnabled = false;

		List<SelectionRect> selectionRects = new List<SelectionRect>();

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
			if (runInBackgroundConfigEnabled) {
				if (!EditorPrefs.GetBool (HistoryBackgroundEnabledPrefKey, false))
					return;
			}
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
//			foreach (var selectionRect in selectionRects) {
//				if (selectionRect.element == null)
//					continue;
//				DragLogic (selectionRect.rect, selectionRect.element);
//			}

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

			DrawRunInBackgroundConfig ();

			DrawHistory();

			DrawPreviousNextButtons ();

			if (GUILayout.Button("Clear")) {
				selectionHistory.Clear();
				Repaint();
			}

			EditorGUILayout.EndScrollView();

			foreach (var selectionRect in selectionRects) {
				if (selectionRect.element == null)
					continue;
				DragLogic (selectionRect.rect, selectionRect.element);
			}
		}

		static void DrawRunInBackgroundConfig ()
		{
			if (!runInBackgroundConfigEnabled)
				return;
			
			var runInBackgroundEnabled = EditorPrefs.GetBool (HistoryBackgroundEnabledPrefKey, false);
			var newRunInBackground = GUILayout.Toggle (runInBackgroundEnabled, "Run in background");

			if (runInBackgroundEnabled && !newRunInBackground) {
				EditorPrefs.SetBool (HistoryBackgroundEnabledPrefKey, false);
				UnregisterSelectionListener ();
			} else if (!runInBackgroundEnabled && newRunInBackground) {
				EditorPrefs.SetBool (HistoryBackgroundEnabledPrefKey, true);
				RegisterSelectionListener ();
			}

		}

		void DrawPreviousNextButtons ()
		{
			if (!prevNextButtonsEnabled)
				return;
			
			if (GUILayout.Button ("Previous")) {
				selectionHistory.Previous ();
				Selection.activeObject = selectionHistory.GetSelection ();
			}

			if (GUILayout.Button ("Next")) {
				selectionHistory.Next ();
				Selection.activeObject = selectionHistory.GetSelection ();
			}
		}

		bool draggingObject = false;

		void DrawHistory()
		{
			selectionRects.Clear ();

			var nonSelectedColor = GUI.contentColor;

			var history = selectionHistory.History;

			for (int i = 0; i < history.Count; i++) {
				var historyElement = history [i];

				if (selectionHistory.IsSelected(i)) {
					GUI.contentColor = new Color(0.2f, 170.0f / 255.0f, 1.0f, 1.0f);
				} else {
					GUI.contentColor = nonSelectedColor;
				}

				var buttonStyle = windowSkin.GetStyle("SelectionButton");

				if (historyElement == null) {
					GUILayout.Button ("Deleted", buttonStyle);
					continue;
				}
					
				var rect = EditorGUILayout.BeginHorizontal ();

				var icon = AssetPreview.GetMiniThumbnail (historyElement);

				GUIContent content = new GUIContent ();

				content.image = icon;
				content.text = historyElement.name;

				if (GUILayout.Button (content, buttonStyle)) {
					if (Event.current.button == 0) {
						UpdateSelection (i);

						// && over current object...
					} else {
						EditorGUIUtility.PingObject (historyElement);
					}
				}

				GUI.contentColor = nonSelectedColor;

				if (GUILayout.Button ("Ping", windowSkin.button)) {
					EditorGUIUtility.PingObject (historyElement);
				}

//				DragLogic (rect, historyElement);

				EditorGUILayout.EndHorizontal ();

				selectionRects.Add (new SelectionRect () {
					rect = rect,
					element = historyElement
				});
			}

			GUI.contentColor = nonSelectedColor;
		}

		void DragLogic(Rect rect, Object currentObject)
		{
			if (currentObject == null)
				return;

			if (Event.current == null)
				return;

//			if (!Event.current.isMouse)
//				return;

			var mousePosition = Event.current.mousePosition;

			if (!draggingObject) {
				var insideRect = rect.Contains (mousePosition);
				var mouseDragging = Event.current.type == EventType.MouseDrag;

				if (!insideRect)
					return;
				
				Debug.Log (string.Format("rect:{0}, drag:{1}", insideRect, mouseDragging));

				if (mouseDragging) {
					DragAndDrop.PrepareStartDrag ();

					DragAndDrop.StartDrag ("MyDragTest");

					DragAndDrop.objectReferences = new Object[] { currentObject };
					DragAndDrop.visualMode = DragAndDropVisualMode.Link;

					draggingObject = true;
				}
			} else {
				if (Event.current.type == EventType.MouseUp) {
					draggingObject = false;
				}
				//					draggingObject = false;
			}
		}

	}
}