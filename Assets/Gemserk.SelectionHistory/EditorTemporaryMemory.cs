using UnityEngine;

namespace Gemserk
{
	public class EditorTemporaryMemory : MonoBehaviour
	{
		static EditorTemporaryMemory instance;

		static HideFlags instanceHideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;

		static void InitTemporaryMemory()
		{
			if (instance != null)
				return;

			var editorMemory = GameObject.Find ("~EditorTemporaryMemory");

			if (editorMemory == null) {
				editorMemory = new GameObject ("~EditorTemporaryMemory");
				instance = editorMemory.AddComponent<EditorTemporaryMemory> ();
			} else {
				instance = editorMemory.GetComponent<EditorTemporaryMemory> ();
				if (instance == null)
					instance = editorMemory.AddComponent<EditorTemporaryMemory> ();
			}

			editorMemory.hideFlags = instanceHideFlags;

			#if UNITY_EDITOR

			var selectionHistoryPersistent = UnityEditor.AssetDatabase.LoadAssetAtPath<SelectionHistoryPersistent>("Assets/Gemserk.SelectionHistory.asset");
			if (selectionHistoryPersistent == null)
			{
				
				selectionHistoryPersistent = ScriptableObject.CreateInstance<SelectionHistoryPersistent>();
				UnityEditor.AssetDatabase.CreateAsset(selectionHistoryPersistent, "Assets/Gemserk.SelectionHistory.asset");
				UnityEditor.AssetDatabase.Refresh();
			}

			instance.selectionHistoryPersistent = selectionHistoryPersistent;
			
			#endif
		}

		public static EditorTemporaryMemory Instance {
			get { 
				InitTemporaryMemory ();
				return instance;
			}
		}

		private SelectionHistoryPersistent selectionHistoryPersistent;
		
	    // [SerializeField]
	    public SelectionHistory selectionHistory => selectionHistoryPersistent.selectionHistory;
	}
	
}