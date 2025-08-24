using System;
using System.IO;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace Gemserk
{
	[InitializeOnLoad]
	public static class SelectionHistoryWindowUtils {

		public static readonly string HistoryAutomaticRemoveDestroyedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveDeleted";
		public static readonly string HistoryAutomaticRemoveUnloadedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveUnloaded";
		
		public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "Gemserk.SelectionHistory.AllowDuplicatedEntries";
	    public static readonly string HistoryShowHierarchyObjectsPrefKey = "Gemserk.SelectionHistory.ShowHierarchyObjects";

	    public static readonly string HistoryShowPinButtonPrefKey = "Gemserk.SelectionHistory.ShowFavoritesPinButton";

	    public static readonly string ShowUnloadedObjectsKey = "Gemserk.SelectionHistory.ShowUnloadedObjects";
	    public static readonly string ShowDestroyedObjectsKey = "Gemserk.SelectionHistory.ShowDestroyedObjects";
	    
	    public static readonly string OrderLastSelectedFirstKey = "Gemserk.SelectionHistory.OrderLastSelectedFirst";
	    public static readonly string BackgroundRecordKey = "Gemserk.SelectionHistory.BackgroundRecord";

	    public const float distanceToConsiderDrag = 10.0f;
	    
	    private static readonly bool debugEnabled = false;
	    
	    static SelectionHistoryWindowUtils()
	    {
		    Selection.selectionChanged += SelectionRecorder;
		    
		    try
		    {
				// This is some kind of migration from old saved files to new version using ScriptableSingleton.

			    var oldSelectionHistoryAssetPath = Path.Combine(Application.dataPath, "Gemserk.SelectionHistory.asset");
			    if (File.Exists(oldSelectionHistoryAssetPath))
			    {
				    Debug.LogWarning($"{oldSelectionHistoryAssetPath} found old history asset file. Auto renaming to avoid editor issues, should be deleted.");
				    var renamedFilePath = Path.Combine(Application.dataPath, "Gemserk.SelectionHistory.backup");
				    FileUtil.MoveFileOrDirectory(oldSelectionHistoryAssetPath, renamedFilePath);
			    }
			    
			    var oldFavoritesAssetPath = Path.Combine(Application.dataPath, "Gemserk.Favorites.asset");
			    if (File.Exists(oldFavoritesAssetPath))
			    {
				    Debug.LogWarning($"{oldFavoritesAssetPath} found old favorites asset file. Auto renaming to avoid editor issues, should be deleted.");
				    var renamedFilePath = Path.Combine(Application.dataPath, "Gemserk.Favorites.backup");
				    FileUtil.MoveFileOrDirectory(oldFavoritesAssetPath, renamedFilePath);
			    }
		    }
		    catch (Exception e) 
		    {
			    Debug.LogError(e);
		    }
	    }
		
	    private static void SelectionRecorder ()
	    {
		    if (!RecordInTheBackground)
			    return;

		    RecordSelectionChange();
	    }

	    public static void RecordSelectionChange()
	    {
		    if (Selection.activeObject != null)
		    {
			    if (debugEnabled)
			    {
				    Debug.Log("Recording new selection: " + Selection.activeObject.name);
			    }

			    var isSceneObject = SelectionHistoryUtils.IsSceneObject(Selection.activeObject);
			    
			    if (!SelectionHistoryWindowUtils.ShowHierarchyViewObjects)
			    {
				    if (isSceneObject)
				    {
					    return;
				    }
			    }

			    if (Application.isPlaying && isSceneObject)
			    {
				    return;
			    }

			    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
			    selectionHistory.UpdateSelection(Selection.activeObject);
		    }
	    }

	    [MenuItem("Window/Gemserk/Previous selection %#,")]
	    [Shortcut("Selection History/Previous Selection")]
	    public static void PreviousSelection()
	    {
		    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		    selectionHistory.Previous ();
		    Selection.activeObject = selectionHistory.GetSelection ();
	    }
	    
	    [MenuItem("Window/Gemserk/Previous selection %#,", true)]
	    public static bool PreviousSelectionValidator()
	    {
		    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		    return selectionHistory.History.Count > 0;
	    }

	    [MenuItem("Window/Gemserk/Next selection %#.")]
	    [Shortcut("Selection History/Next Selection")]
	    public static void NextSelection()
	    {
		    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		    selectionHistory.Next();
		    Selection.activeObject = selectionHistory.GetSelection ();
	    }
	    
	    [MenuItem("Window/Gemserk/Next selection %#.", true)]
	    public static bool NextSelectionValidator()
	    {
		    var selectionHistory = SelectionHistoryAsset.instance.selectionHistory;
		    return selectionHistory.History.Count > 0;
	    }
		
		public static bool AutomaticRemoveDestroyed =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveDestroyedPrefKey, true);
		
		public static bool AutomaticRemoveUnloaded =>
			EditorPrefs.GetBool(HistoryAutomaticRemoveUnloadedPrefKey, true);
		
		public static bool AllowDuplicatedEntries =>
			EditorPrefs.GetBool(HistoryAllowDuplicatedEntriesPrefKey, false);

		public static bool ShowHierarchyViewObjects =>
			EditorPrefs.GetBool(HistoryShowHierarchyObjectsPrefKey, true);
		
		public static bool ShowUnloadedObjects =>
			EditorPrefs.GetBool(ShowUnloadedObjectsKey, true);
		
		public static bool ShowDestroyedObjects =>
			EditorPrefs.GetBool(ShowDestroyedObjectsKey, false);
		
		public static bool ShowFavoriteButton =>
			EditorPrefs.GetBool(HistoryShowPinButtonPrefKey, false);
		
		public static bool OrderLastSelectedFirst =>
			EditorPrefs.GetBool(OrderLastSelectedFirstKey, false);
		
		public static bool RecordInTheBackground =>
			EditorPrefs.GetBool(BackgroundRecordKey, false);
	

	    public static void PingEntry(SelectionHistory.Entry e)
	    {
		    if (e.GetReferenceState() == SelectionHistory.Entry.State.ReferenceUnloaded)
		    {
			    var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(e.scenePath);
			    EditorGUIUtility.PingObject(sceneAsset);
		    } else
		    {
			    EditorGUIUtility.PingObject(e.Reference);
		    }
	    }
	}
}