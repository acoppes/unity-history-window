using UnityEngine;
using UnityEditor;

namespace Gemserk
{
	public class SelectionHistoryWindow : EditorWindow, IHasCustomMenu {
		private const float buttonsWidth = 120f;

		public static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";
		public static readonly string HistoryAutomaticRemoveDeletedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveDeleted";
		public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "Gemserk.SelectionHistory.AllowDuplicatedEntries";
	    public static readonly string HistoryShowHierarchyObjectsPrefKey = "Gemserk.SelectionHistory.ShowHierarchyObjects";
	    public static readonly string HistoryShowProjectViewObjectsPrefKey = "Gemserk.SelectionHistory.ShowProjectViewObjects";

	    public static readonly string HistoryShowPinButtonPrefKey = "Gemserk.SelectionHistory.ShowFavoritesPinButton";

	    public static readonly string ShowUnloadedObjectsKey = "Gemserk.SelectionHistory.ShowUnloadedObjects";
	    public static readonly string ShowDestroyedObjectsKey = "Gemserk.SelectionHistory.ShowDestroyedObjects";
	    
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
			
			var titleContent = EditorGUIUtility.IconContent(UnityBuiltInIcons.refreshIconName);
			titleContent.text = "History";
			titleContent.tooltip = "Objects selection history";
			window.titleContent = titleContent;
			
			window.Show();
		}

        public GUISkin windowSkin;

		private void OnEnable()
		{
			selectionHistory = EditorTemporaryMemory.Instance.selectionHistory;
			selectionHistory.HistorySize = EditorPrefs.GetInt (HistorySizePrefKey, 10);

			Selection.selectionChanged += delegate {

				if (selectionHistory.IsSelected(selectionHistory.GetHistoryCount() - 1)) {
					_historyScrollPosition.y = float.MaxValue;
				}

				Repaint();
			};
		}

		private void UpdateSelection(Object obj)
		{
		    selectionHistory.SetSelection(obj);
            Selection.activeObject = obj;
            // Selection.activeObject = selectionHistory.UpdateSelection(currentIndex);
		}

	    private Vector2 _favoritesScrollPosition;
		private Vector2 _historyScrollPosition;

		private bool showProjectViewObjects;
		
		public static bool AutomaticRemoveDeleted =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveDeletedPrefKey, true);
		
		public static bool AllowDuplicatedEntries =>
			EditorPrefs.GetBool(HistoryAllowDuplicatedEntriesPrefKey, false);

		public static bool ShowHierarchyViewObjects =>
			EditorPrefs.GetBool(HistoryShowHierarchyObjectsPrefKey, true);
		
		public static bool ShowUnloadedObjects =>
			EditorPrefs.GetBool(ShowUnloadedObjectsKey, true);
		
		public static bool ShowDestroyedObjects =>
			EditorPrefs.GetBool(ShowDestroyedObjectsKey, false);
		
		private void OnGUI () {

			if (shouldReloadPreferences) {
				selectionHistory.HistorySize = EditorPrefs.GetInt (SelectionHistoryWindow.HistorySizePrefKey, 10);
			    showProjectViewObjects = EditorPrefs.GetBool(SelectionHistoryWindow.HistoryShowProjectViewObjectsPrefKey, true);

                shouldReloadPreferences = false;
			}

			if (AutomaticRemoveDeleted)
				selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);

			if (!AllowDuplicatedEntries)
				selectionHistory.RemoveDuplicated();

			var showUnloaded = EditorPrefs.GetBool (ShowUnloadedObjectsKey, true);
            var showDestroyed = EditorPrefs.GetBool (ShowDestroyedObjectsKey, false);
        
            var changedBefore = GUI.changed;

			_historyScrollPosition = EditorGUILayout.BeginScrollView(_historyScrollPosition);

			var changedAfter = GUI.changed;

			if (!changedBefore && changedAfter) {
				Debug.Log ("changed");
			}

			DrawHistory();

			EditorGUILayout.EndScrollView();

			if (GUILayout.Button("Clear")) {
				selectionHistory.Clear();
				Repaint();
			}

			if (!AutomaticRemoveDeleted && showDestroyed) {
				if (GUILayout.Button ("Remove Destroyed")) {
					selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceDestroyed);
					Repaint();
				}
			} 
			
			if (showUnloaded) {
				if (GUILayout.Button ("Remove Unloaded")) {
					selectionHistory.RemoveEntries(SelectionHistory.Entry.State.ReferenceUnloaded);
					Repaint();
				}
			} 

			if (AllowDuplicatedEntries) {
				if (GUILayout.Button ("Remove Duplicated")) {
					selectionHistory.RemoveDuplicated ();
					Repaint();
				}
			}
		}



		private void DrawElement(SelectionHistory.Entry e, Color originalColor, 
			bool showUnloaded, bool showDestroyed, bool appendScene)
	    {
	        var buttonStyle = windowSkin.GetStyle("SelectionButton");
			buttonStyle.fixedWidth = position.width - buttonsWidth;
			var nonSelectedColor = originalColor;

			var obj = e.reference;

			if (!EditorUtility.IsPersistent(obj))
            {
                if (!ShowHierarchyViewObjects)
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
	            if (showDestroyed)
	            {
		            GUI.contentColor = unreferencedObjectColor;
		            GUILayout.Label(new GUIContent()
		            {
			            text = $"{e.GetName(appendScene)} (Destroyed)",
			            tooltip = $"Object destroyed or referenced lost."
		            }, buttonStyle);
	            }
            } else if (e.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
            {
	            if (showUnloaded)
	            {
		            GUI.contentColor = unreferencedObjectColor;
		            GUILayout.Label(new GUIContent()
		            {
			            text = e.GetName(appendScene),
			            tooltip = $"Object from unloaded scene {e.sceneName}"
		            }, buttonStyle);
		            
		            GUI.contentColor = originalColor;
		            if (GUILayout.Button("Ping", windowSkin.button))
		            {
			            PingEntry(e);
		            }
	            }
            }
            else
            {
                var icon = AssetPreview.GetMiniThumbnail(obj);

                var content = new GUIContent
                {
	                image = icon, 
	                text = e.GetName(appendScene)
                };

                // chnanged to label to be able to handle events for drag
                GUILayout.Label(content, buttonStyle);

                GUI.contentColor = originalColor;
                if (GUILayout.Button("Ping", windowSkin.button))
                {
	                PingEntry(e);
                }

                var showPinFavoriteButton = EditorPrefs.GetBool(HistoryShowPinButtonPrefKey, true);
                
                var favorites = FavoritesController.Favorites;
                if (showPinFavoriteButton && favorites.CanBeFavorite(e.reference))
                {
	                
	                var pinString = "Pin";
                    // var isFavorite = e.isFavorite;
                    
                    var isFavorite = favorites.IsFavorite(e.reference);
                    
                    if (isFavorite)
                    {
                        pinString = "Unpin";
                    }

                    if (GUILayout.Button(pinString, windowSkin.button))
                    {
	                    favorites.ToggleFavorite(e.reference);
	                    Repaint();
                    }
                }

            }

            EditorGUILayout.EndHorizontal();

            ButtonLogic(rect, e);
        }

		private void DrawHistory()
		{
			var originalColor = GUI.contentColor;

			var history = selectionHistory.History;
			
			var showUnloaded = EditorPrefs.GetBool (ShowUnloadedObjectsKey, true);
		    var showDestroyed = EditorPrefs.GetBool (ShowDestroyedObjectsKey, false);

		    if (!ShowHierarchyViewObjects)
		    {
			    showUnloaded = false;
		    }
		    
            foreach (var historyElement in history)
            {
	            DrawElement(historyElement, originalColor, showUnloaded, showDestroyed, true);
            }

			GUI.contentColor = originalColor;
		}

	    private void ButtonLogic(Rect rect, SelectionHistory.Entry e)
		{
			var currentEvent = Event.current;
			var currentObject = e.reference;

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

				if (Event.current.button == 0) {
					if (currentObject != null)
					{
						UpdateSelection(currentObject);
					}
				} else {
					PingEntry(e);
				}
				
				Event.current.Use ();
			}

		}

	    public static void PingEntry(SelectionHistory.Entry e)
	    {
		    if (e.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
		    {
			    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(e.scenePath);
			    EditorGUIUtility.PingObject(sceneAsset);
		    } else
		    {
			    EditorGUIUtility.PingObject(e.reference);
		    }
	    }

	    public void AddItemsToMenu(GenericMenu menu)
	    {
		    AddMenuItemForPreference(menu, HistoryShowHierarchyObjectsPrefKey, "HierarchyView Objects", 
			    "Toggle to show/hide objects from scene hierarchy view.");
		 
		    if (ShowHierarchyViewObjects)
		    {
			    AddMenuItemForPreference(menu, ShowUnloadedObjectsKey, "Unloaded Objects", 
				    "Toggle to show/hide unloaded objects from scenes hierarchy view.");
		    } 
		    
		    AddMenuItemForPreference(menu, ShowDestroyedObjectsKey, "Destroyed Objects", 
			    "Toggle to show/hide unreferenced or destroyed objects.");
		    
		    menu.AddItem(new GUIContent("Open preferences"), false, delegate
		    {
			    SettingsService.OpenUserPreferences("Selection History");
		    });
	    }

	    private void AddMenuItemForPreference(GenericMenu menu, string preference, string text, string tooltip)
	    {
		    const bool defaultValue = true;
		    var value = EditorPrefs.GetBool(preference, defaultValue);
		    var name = value ? $"Hide {text}" : $"Show {text}";
		    menu.AddItem(new GUIContent(name, tooltip), false, delegate
		    {
			    ToggleBoolEditorPref(preference, defaultValue);
			    shouldReloadPreferences = true;
			    Repaint();
		    });
	    }

	    private static void ToggleBoolEditorPref(string preferenceName, bool defaultValue)
	    {
		    var newValue = !EditorPrefs.GetBool(preferenceName, defaultValue);
		    EditorPrefs.SetBool(preferenceName, newValue);
		    // return newValue;
	    }
	}
}