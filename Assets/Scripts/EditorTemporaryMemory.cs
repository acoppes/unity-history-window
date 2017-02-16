using UnityEngine;
using System.Collections.Generic;

namespace Gemserk
{
	public class EditorTemporaryMemory : MonoBehaviour
	{
		static EditorTemporaryMemory instance;

		static void InitTemporaryMemory()
		{
			if (instance != null)
				return;

			var editorMemory = GameObject.Find ("~EditorTemporaryMemory");

			if (editorMemory == null) {
				editorMemory = new GameObject ("~EditorTemporaryMemory");
				editorMemory.hideFlags = HideFlags.DontSave;

				instance = editorMemory.AddComponent<EditorTemporaryMemory> ();
			} else {
				instance = editorMemory.GetComponent<EditorTemporaryMemory> ();
				if (instance == null)
					instance = editorMemory.AddComponent<EditorTemporaryMemory> ();
			}
		}

		public static EditorTemporaryMemory Instance {
			get { 
				InitTemporaryMemory ();
				return instance;
			}
		}

		[SerializeField]
		public List<Object> history = new List<Object>(100);
	}
	
}