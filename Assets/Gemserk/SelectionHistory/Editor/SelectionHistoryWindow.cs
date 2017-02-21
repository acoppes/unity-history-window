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

		static readonly bool debugEnabled = false;

		static readonly bool prevNextButtonsEnabled = false;

		static readonly bool runInBackgroundConfigEnabled = false;

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

				if (selectionHistory.IsSelected(selectionHistory.GetHistoryCount() - 1)) {
					scrollPosition.y = float.MaxValue;
				}

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

		bool autoclearDeleted;

		void OnGUI () {

			int currentHistorySize = selectionHistory.HistorySize;

			selectionHistory.HistorySize = EditorGUILayout.IntField("History Size", selectionHistory.HistorySize);

			if (selectionHistory.HistorySize != currentHistorySize) {
				// updates user pref for history size
				EditorPrefs.SetInt(HistorySizePrefKey, selectionHistory.HistorySize);
			}

			autoclearDeleted = EditorGUILayout.Toggle ("Automatic remove deleted", autoclearDeleted);

			if (autoclearDeleted) {
				selectionHistory.ClearDeleted ();
			}

			DrawRunInBackgroundConfig ();

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			DrawHistory();

			EditorGUILayout.EndScrollView();

			DrawPreviousNextButtons ();

			if (GUILayout.Button("Clear")) {
				selectionHistory.Clear();
				Repaint();
			}

			if (!autoclearDeleted) {
				if (GUILayout.Button ("Remove Deleted")) {
					selectionHistory.ClearDeleted ();
					Repaint ();
				}
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

		void DrawHistory()
		{
			var nonSelectedColor = GUI.contentColor;

			var history = selectionHistory.History;

			var buttonStyle = windowSkin.GetStyle("SelectionButton");

			for (int i = 0; i < history.Count; i++) {
				var historyElement = history [i];

				if (selectionHistory.IsSelected(i)) {
					GUI.contentColor = new Color(0.2f, 170.0f / 255.0f, 1.0f, 1.0f);
				} else {
					GUI.contentColor = nonSelectedColor;
				}

				var rect = EditorGUILayout.BeginHorizontal ();

				if (historyElement == null) {
					GUILayout.Label ("Deleted", buttonStyle); 
				} else {

					var icon = AssetPreview.GetMiniThumbnail (historyElement);

					GUIContent content = new GUIContent ();

					content.image = icon;
					content.text = historyElement.name;

					// chnanged to label to be able to handle events for drag
					GUILayout.Label (content, buttonStyle); 

					GUI.contentColor = nonSelectedColor;

					if (GUILayout.Button ("Ping", windowSkin.button)) {
						EditorGUIUtility.PingObject (historyElement);
					}

				}
					
				EditorGUILayout.EndHorizontal ();

				ButtonLogic (i, rect, historyElement);
			}

			GUI.contentColor = nonSelectedColor;
		}

		void ButtonLogic(int currentIndex, Rect rect, Object currentObject)
		{
			var currentEvent = Event.current;

			if (currentEvent == null)
				return;

			if (!rect.Contains (currentEvent.mousePosition))
				return;
			
//			Debug.Log (string.Format("event:{0}", currentEvent.ToString()));

			var eventType = currentEvent.type;

			if (eventType == EventType.MouseDrag) {

				if (currentObject != null) {
					DragAndDrop.PrepareStartDrag ();

					DragAndDrop.StartDrag (currentObject.name);

					DragAndDrop.objectReferences = new Object[] { currentObject };

//					if (ProjectWindowUtil.IsFolder(currentObject.GetInstanceID())) {

					// fixed to use IsPersistent to work with all assets with paths.
					if (EditorUtility.IsPersistent(currentObject)) {

						// added DragAndDrop.path in case we are dragging a folder.

						DragAndDrop.paths = new string[] {
							AssetDatabase.GetAssetPath(currentObject)
						};

						// previous test with setting generic data by looking at
						// decompiled Unity code.

						// DragAndDrop.SetGenericData ("IsFolder", "isFolder");
					}
				}

				Event.current.Use ();

			} else if (eventType == EventType.MouseUp) {

				if (currentObject != null) {
					if (Event.current.button == 0) {
						UpdateSelection (currentIndex);
					} else {
						EditorGUIUtility.PingObject (currentObject);
					}
				}

				Event.current.Use ();
			}

		}

	}
}