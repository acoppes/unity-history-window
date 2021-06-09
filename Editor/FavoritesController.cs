using UnityEditor;
using UnityEngine;

namespace Gemserk
{
    public class FavoritesController
    {
        private static Favorites _favoritesInstance;
    
        public static Favorites Favorites
        {
            get
            {
                if (_favoritesInstance == null)
                {
                    var assetPath = "Assets/Gemserk.Favorites.asset";
                    
                    _favoritesInstance = AssetDatabase.LoadAssetAtPath<Favorites>(assetPath);
                    _favoritesInstance.ClearUnreferenced();

                    if (_favoritesInstance == null)
                    {
                        _favoritesInstance = ScriptableObject.CreateInstance<Favorites>();
                        AssetDatabase.CreateAsset(_favoritesInstance, assetPath);
                        AssetDatabase.SaveAssets();
                    }
                    
                    _favoritesInstance.OnFavoritesUpdated += OnFavoritesUpdated;
                }
                return _favoritesInstance;
            }
        }

        private static void OnFavoritesUpdated(Favorites favorites)
        {
            EditorUtility.SetDirty(favorites);
            AssetDatabase.SaveAssets();
        }
    }
}