using UnityEngine;

namespace Gemserk
{
    [UnityEditor.InitializeOnLoad]
    public static class SelectionHistoryReference
    {
        private const string AssetsGemserkSelectionhistoryAsset = "Assets/Gemserk.SelectionHistory.asset";
        
        static SelectionHistoryAsset instance;

        public static SelectionHistory SelectionHistory => instance.selectionHistory; 
		
        static SelectionHistoryReference()
        {
            instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SelectionHistoryAsset>(AssetsGemserkSelectionhistoryAsset);
            
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<SelectionHistoryAsset>();
                UnityEditor.AssetDatabase.CreateAsset(instance, AssetsGemserkSelectionhistoryAsset);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
}