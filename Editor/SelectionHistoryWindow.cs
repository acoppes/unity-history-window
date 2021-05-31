using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine.SceneManagement;

namespace Gemserk
{
	[InitializeOnLoad]
	public static class SelectionHistoryInitialization
	{
		static SelectionHistoryInitialization()
		{
			SelectionHistoryWindow.RegisterSelectionListener();
		}
	}

	[InitializeOnLoad]
	public static class StoreSceneSelection
	{
		static StoreSceneSelection()
		{
			EditorSceneManager.sceneClosing += StoreSceneSelectionOnSceneClosing;
			EditorSceneManager.sceneOpened += StoreSceneSelectionOnSceneOpened;
		}

		private static void StoreSceneSelectionOnSceneOpened(Scene scene, OpenSceneMode mode)
		{
			var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;

			if (selectionHistory == null)
				return;
			
			var entries = selectionHistory.History;

			foreach (var entry in entries)
			{
				if (!string.IsNullOrEmpty(entry.globalObjectId))
				{
					// This only parses the global id but that doesnt mean its object is not null
					if (GlobalObjectId.TryParse(entry.globalObjectId, out var globalObjectId))
					{
						var reference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
						if (reference != null)
						{
							// Debug.Log($"Restoring scene object reference {entry.name} from GlobalId");
							entry.reference = GlobalObjectId.GlobalObjectIdentifierToObjectSlow(globalObjectId);
							entry.globalObjectId = null;
						}
					}
				}
			}
		}

		private static void StoreSceneSelectionOnSceneClosing(Scene scene, bool removingScene)
		{
			if (!removingScene)
				return;
			
			var selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;

			if (selectionHistory == null)
				return;
			
			var entries = selectionHistory.History;
			foreach (var entry in entries)
			{
				if (entry.reference != null && entry.reference is GameObject go)
				{
					// GameObject's scene is being unloaded here...
					if (go.scene == scene)
					{
						entry.sceneName = scene.name;
						entry.globalObjectId = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
						// Debug.Log($"Storing scene object reference {entry.name} as GlobalId");
						// entry.state = SelectionHistory.Entry.State.ReferenceUnloaded;
					}
				}
			}
		}
	}

	public class SelectionHistoryWindow : EditorWindow {
		private const float buttonsWidth = 120f;

		public static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";
		public static readonly string HistoryAutomaticRemoveDeletedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveDeleted";
		public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "Gemserk.SelectionHistory.AllowDuplicatedEntries";
	    public static readonly string HistoryShowHierarchyObjectsPrefKey = "Gemserk.SelectionHistory.ShowHierarchyObjects";
	    public static readonly string HistoryShowProjectViewObjectsPrefKey = "Gemserk.SelectionHistory.ShowProjectViewObjects";
	    public static readonly string HistoryFavoritesPrefKey = "Gemserk.SelectionHistory.Favorites";

	    private static SelectionHistory selectionHistory = new SelectionHistory();

	    private static readonly bool debugEnabled = false;

		public static bool shouldReloadPreferences = true;

	    private static Color hierarchyElementColor = new Color(0.7f, 1.0f, 0.7f);
	    private static Color selectedElementColor = new Color(0.2f, 170.0f / 255.0f, 1.0f, 1.0f);
	    private static Color unreferencedObjectColor = new Color(0.4f, 0.4f, 0.4f);
	    
        [MenuItem ("Window/Gemserk/Selection History %#h")]
        // [Shortcut("Selection History/Open Selection Hist", KeyCode.Mouse4)]
        private static void Init () {
			// Get existing open window or if none, make a new one:
			var window = EditorWindow.GetWindow<SelectionHistoryWindow> ();

			window.titleContent.text = "History";
			window.Show();
		}

        private static void SelectionRecorder ()
		{
			if (Selection.activeObject != null) {
				if (debugEnabled) {
					Debug.Log ("Recording new selection: " + Selection.activeObject.name);
				}

				selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
				selectionHistory.UpdateSelection (Selection.activeObject);
			} 
		}

		public static void RegisterSelectionListener()
		{
			Selection.selectionChanged += SelectionRecorder;
		}

		public GUISkin windowSkin;

		private MethodInfo openPreferencesWindow;

		private void OnEnable()
		{
			automaticRemoveDeleted = EditorPrefs.GetBool (HistoryAutomaticRemoveDeletedPrefKey, true);

			selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
			selectionHistory.HistorySize = EditorPrefs.GetInt (HistorySizePrefKey, 10);

			Selection.selectionChanged += delegate {

				if (selectionHistory.IsSelected(selectionHistory.GetHistoryCount() - 1)) {
					_historyScrollPosition.y = float.MaxValue;
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

		private void UpdateSelection(Object obj)
		{
		    selectionHistory.SetSelection(obj);
            Selection.activeObject = obj;
            // Selection.activeObject = selectionHistory.UpdateSelection(currentIndex);
		}

	    private Vector2 _favoritesScrollPosition;
		private Vector2 _historyScrollPosition;

		private bool automaticRemoveDeleted;
		private bool allowDuplicatedEntries;

		private bool showHierarchyViewObjects;
		private bool showProjectViewObjects;

		private void OnGUI () {

			if (shouldReloadPreferences) {
				selectionHistory.HistorySize = EditorPrefs.GetInt (SelectionHistoryWindow.HistorySizePrefKey, 10);
				automaticRemoveDeleted = EditorPrefs.GetBool (SelectionHistoryWindow.HistoryAutomaticRemoveDeletedPrefKey, true);
				allowDuplicatedEntries = EditorPrefs.GetBool (SelectionHistoryWindow.HistoryAllowDuplicatedEntriesPrefKey, false);

			    showHierarchyViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowHierarchyObjectsPrefKey, true);
			    showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, true);

                shouldReloadPreferences = false;
			}

			if (automaticRemoveDeleted)
				selectionHistory.ClearDeleted ();

			if (!allowDuplicatedEntries)
				selectionHistory.RemoveDuplicated ();

            var favoritesEnabled = EditorPrefs.GetBool(HistoryFavoritesPrefKey, true);
            if (favoritesEnabled && selectionHistory.HasFavorites)
            {
                _favoritesScrollPosition = EditorGUILayout.BeginScrollView(_favoritesScrollPosition);
                DrawFavorites();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.Separator();
            }
        
            bool changedBefore = GUI.changed;

			_historyScrollPosition = EditorGUILayout.BeginScrollView(_historyScrollPosition);

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
				if (GUILayout.Button ("Remove Unreferenced")) {
					selectionHistory.ClearDeleted ();
					Repaint ();
				}
			} 

			if (allowDuplicatedEntries) {
				if (GUILayout.Button ("Remove Duplicated")) {
					selectionHistory.RemoveDuplicated ();
					Repaint ();
				}
			} 

			DrawSettingsButton ();
		}

		private void DrawSettingsButton()
		{
			if (openPreferencesWindow == null)
				return;
			
			if (GUILayout.Button ("Preferences")) {
				openPreferencesWindow.Invoke(null, null);
			}
		}
			
		[MenuItem("Window/Gemserk/Previous selection %#,")]
		[Shortcut("Selection History/Previous Selection")]
		public static void PreviousSelection()
		{
			selectionHistory.Previous ();
			Selection.activeObject = selectionHistory.GetSelection ();
		}

		[MenuItem("Window/Gemserk/Next selection %#.")]
		[Shortcut("Selection History/Next Selection")]
		public static void Nextelection()
		{
			selectionHistory.Next();
			Selection.activeObject = selectionHistory.GetSelection ();
		}

		private void DrawElement(SelectionHistory.Entry e, int i, Color originalColor)
	    {
	        var buttonStyle = windowSkin.GetStyle("SelectionButton");
			buttonStyle.fixedWidth = position.width - buttonsWidth;
			var nonSelectedColor = originalColor;

			var obj = e.reference;

			if (!EditorUtility.IsPersistent(obj))
            {
                if (!showHierarchyViewObjects)
                    return;
                nonSelectedColor = hierarchyElementColor;
            }
            else
            {
                if (!showProjectViewObjects)
                    return;
            }

            if (selectionHistory.IsSelected(obj))
            {
                GUI.contentColor = selectedElementColor;
            }
            else
            {
                GUI.contentColor = nonSelectedColor;
            }

            var rect = EditorGUILayout.BeginHorizontal();

            if (e.GetReferenceState() == SelectionHistory.Entry.State.ReferenceDestroyed)
            {
	            GUI.contentColor = unreferencedObjectColor;
                GUILayout.Label(e.name, buttonStyle);
            } else if (e.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
            {
	            GUI.contentColor = unreferencedObjectColor;
	            GUILayout.Label($"Scene:{e.sceneName}/{e.name}", buttonStyle);
            }
            else
            {
                var icon = AssetPreview.GetMiniThumbnail(obj);

                var content = new GUIContent
                {
	                image = icon, 
	                text = obj.name
                };
                
                // chnanged to label to be able to handle events for drag
                GUILayout.Label(content, buttonStyle);

                GUI.contentColor = originalColor;

                if (GUILayout.Button("Ping", windowSkin.button))
                {
                    EditorGUIUtility.PingObject(obj);
                }

                var favoritesEnabled = EditorPrefs.GetBool(HistoryFavoritesPrefKey, true);

                if (favoritesEnabled)
                {
                    var pinString = "Pin";
                    var isFavorite = e.isFavorite;

                    if (isFavorite)
                    {
                        pinString = "Unpin";
                    }

                    if (GUILayout.Button(pinString, windowSkin.button))
                    {
	                    e.ToggleFavorite();
                        Repaint();
                    }
                }

            }

            EditorGUILayout.EndHorizontal();

            ButtonLogic(rect, obj);
        }

		private void DrawFavorites()
	    {
	        var originalColor = GUI.contentColor;

	        var entries = selectionHistory.History;

	        // var buttonStyle = windowSkin.GetStyle("SelectionButton");

	        for (var i = 0; i < entries.Count; i++)
	        {
	            var favorite = entries[i];
	            if (!favorite.isFavorite)
		            continue;
                DrawElement(favorite, i, originalColor);
	        }

	        GUI.contentColor = originalColor;
        }

	    private void DrawHistory()
		{
			var originalColor = GUI.contentColor;

			var history = selectionHistory.History;

			// var buttonStyle = windowSkin.GetStyle("SelectionButton");

		    var favoritesEnabled = EditorPrefs.GetBool(HistoryFavoritesPrefKey, true);

            for (var i = 0; i < history.Count; i++) {
				var historyElement = history [i];
                if (historyElement.isFavorite && favoritesEnabled)
                    continue;
			    DrawElement(historyElement, i, originalColor);
            }

			GUI.contentColor = originalColor;
		}

	    private void ButtonLogic(Rect rect, Object currentObject)
		{
			var currentEvent = Event.current;

			if (currentEvent == null)
				return;

			if (!rect.Contains (currentEvent.mousePosition))
				return;
			
//			Debug.Log (string.Format("event:{0}", currentEvent.ToString()));

			var eventType = currentEvent.type;

			if (eventType == EventType.MouseDrag && currentEvent.button == 0) {
				
				#if !UNITY_EDITOR_OSX
				
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
				#endif

			} else if (eventType == EventType.MouseUp) {

				if (currentObject != null) {
					if (Event.current.button == 0) {
						UpdateSelection (currentObject);
					} else {
						EditorGUIUtility.PingObject (currentObject);
					}
				}

				Event.current.Use ();
			}

		}

	}
}