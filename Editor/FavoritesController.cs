using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    public class FavoritesController
    {
        private static FavoritesAsset _favoritesInstance;
    
        public static FavoritesAsset Favorites
        {
            get
            {
                if (_favoritesInstance == null)
                {
                    var assetPath = "Assets/Gemserk.Favorites.asset";
                    
                    _favoritesInstance = AssetDatabase.LoadAssetAtPath<FavoritesAsset>(assetPath);
                    
                    if (_favoritesInstance == null)
                    {
                        _favoritesInstance = ScriptableObject.CreateInstance<FavoritesAsset>();
                        AssetDatabase.CreateAsset(_favoritesInstance, assetPath);
                        AssetDatabase.SaveAssets();
                    }
                    
                    _favoritesInstance.ClearUnreferenced();
                    _favoritesInstance.OnFavoritesUpdated += OnFavoritesUpdated;
                }
                return _favoritesInstance;
            }
        }

        private static void OnFavoritesUpdated(FavoritesAsset favorites)
        {
            EditorUtility.SetDirty(favorites);
            AssetDatabase.SaveAssets();
        }
    }
}