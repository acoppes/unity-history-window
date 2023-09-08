using UnityEngine;

namespace Gemserk
{
    [UnityEditor.InitializeOnLoad]
    public static class SelectionHistoryReference
    {
        // TODO: make it configurable?
        
        private const string AssetsGemserkSelectionHistoryAsset = "Assets/Gemserk.SelectionHistory.asset";
        
        static SelectionHistoryAsset instance;

        public static SelectionHistory SelectionHistory => instance.selectionHistory;

        public static SelectionHistoryAsset asset;
		
        static SelectionHistoryReference()
        {
            instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SelectionHistoryAsset>(AssetsGemserkSelectionHistoryAsset);
            
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<SelectionHistoryAsset>();
                UnityEditor.AssetDatabase.CreateAsset(instance, AssetsGemserkSelectionHistoryAsset);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
}