using UnityEditor;

namespace Gemserk
{
    public class RefreshDataOnAssetDeletedPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (deletedAssets.Length > 0)
            {
                if (EditorWindow.HasOpenInstances<SelectionHistoryWindow>())
                {
                    var window = EditorWindow.GetWindow<SelectionHistoryWindow>();
                    window.ReloadRootAndRemoveUnloadedAndDuplicated();
                }
                
                if (EditorWindow.HasOpenInstances<FavoriteAssetsWindow>())
                {
                    var window = EditorWindow.GetWindow<FavoriteAssetsWindow>();
                    window.RefreshView();
                }
            }
        }
    }
}