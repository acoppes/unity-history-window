using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

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

		public static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";
		public static readonly string HistoryAutomaticRemoveDeletedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveDeleted";
		public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "Gemserk.SelectionHistory.AllowDuplicatedEntries";

		static readonly SelectionHistory selectionHistory = new SelectionHistory();

		static readonly bool debugEnabled = false;

		public static bool shouldReloadPreferences = true;

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
			Selection.selectionChanged += SelectionRecorder;
		}

		public GUISkin windowSkin;

		MethodInfo openPreferencesWindow;

		void OnEnable()
		{
			automaticRemoveDeleted = EditorPrefs.GetBool (HistoryAutomaticRemoveDeletedPrefKey, true);

			selectionHistory.History = EditorTemporaryMemory.Instance.history;
			selectionHistory.HistorySize = EditorPrefs.GetInt (HistorySizePrefKey, 10);

			Selection.selectionChanged += delegate {

				if (selectionHistory.IsSelected(selectionHistory.GetHistoryCount() - 1)) {
					scrollPosition.y = float.MaxValue;
				}

				Repaint();
			};

			try {
				var asm = Assembly.GetAssembly (typeof(EditorWindow));
				var t = asm.GetType ("UnityEditor.PreferencesWindow");
				openPreferencesWindow = t.GetMethod ("ShowPreferencesWindow", BindingFlags.NonPublic | BindingFlags.Static);
			} catch {
				// couldnt get preferences window...
				openPreferencesWindow = null;
			}
		}

		void UpdateSelection(int currentIndex)
		{
			Selection.activeObject = selectionHistory.UpdateSelection(currentIndex);
		}

		Vector2 scrollPosition;

		bool automaticRemoveDeleted;
		bool allowDuplicatedEntries;

		void OnGUI () {

			if (shouldReloadPreferences) {
				selectionHistory.HistorySize = EditorPrefs.GetInt (SelectionHistoryWindow.HistorySizePrefKey, 10);
				automaticRemoveDeleted = EditorPrefs.GetBool (SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
				allowDuplicatedEntries = EditorPrefs.GetBool (SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);
				shouldReloadPreferences = false;
			}

			if (automaticRemoveDeleted)
				selectionHistory.ClearDeleted ();

			if (!allowDuplicatedEntries)
				selectionHistory.RemoveDuplicated ();

			bool changedBefore = GUI.changed;

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

			bool changedAfter = GUI.changed;

			if (!changedBefore && changedAfter) {
				Debug.Log ("changed");
			}

			DrawHistory();

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Clear")) {
				selectionHistory.Clear();
				Repaint();
			}

			if (!automaticRemoveDeleted) {
				if (GUILayout.Button ("Remove Deleted")) {
					selectionHistory.ClearDeleted ();
					Repaint ();
				}
			} 

			if (allowDuplicatedEntries) {
				if (GUILayout.Button ("Remove Duplciated")) {
					selectionHistory.RemoveDuplicated ();
					Repaint ();
				}
			} 

			DrawSettingsButton ();
		}

		void DrawSettingsButton()
		{
			if (openPreferencesWindow == null)
				return;
			
			if (GUILayout.Button ("Preferences")) {
				openPreferencesWindow.Invoke(null, null);
			}
		}
			
		[MenuItem("Window/Gemserk/Previous selection %#,")]
		public static void PreviousSelection()
		{
			selectionHistory.Previous ();
			Selection.activeObject = selectionHistory.GetSelection ();
		}

		[MenuItem("Window/Gemserk/Next selection %#.")]
		public static void Nextelection()
		{
			selectionHistory.Next();
			Selection.activeObject = selectionHistory.GetSelection ();
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