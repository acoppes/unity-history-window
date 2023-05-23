using UnityEngine;

namespace Gemserk
{
    [UnityEditor.InitializeOnLoad]
    public static class SelectionHistoryReference
    {
        private const string AssetsGemserkSelectionhistoryAsset = "Assets/Gemserk.SelectionHistory.asset";
        
        static SelectionHistoryPersistent instance;

        public static SelectionHistory SelectionHistory => instance.selectionHistory; 
		
        static SelectionHistoryReference()
        {
            instance = UnityEditor.AssetDatabase.LoadAssetAtPath<SelectionHistoryPersistent>(AssetsGemserkSelectionhistoryAsset);
            
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<SelectionHistoryPersistent>();
                UnityEditor.AssetDatabase.CreateAsset(instance, AssetsGemserkSelectionhistoryAsset);
                UnityEditor.AssetDatabase.Refresh();
            }
        }
    }
}