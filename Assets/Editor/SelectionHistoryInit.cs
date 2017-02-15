using UnityEditor;

namespace Gemserk
{
	[InitializeOnLoad]
	public class SelectionHistoryInit {

		static SelectionHistoryInit()
		{
			SelectionHistoryWindow.RegisterSelectionListener ();
		}

	}
}