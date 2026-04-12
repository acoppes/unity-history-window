using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
    [FilePath("Gemserk/Gemserk.Favorites.asset", FilePathAttribute.Location.ProjectFolder)]
    public class FavoritesAsset : ScriptableSingleton<FavoritesAsset>
    {
        [Serializable]
        public class Favorite
        {
            public LazyLoadReference<Object> reference;
            public string assetPath;
        }
    
        public event Action<FavoritesAsset> OnFavoritesUpdated;

        [SerializeField]
        public List<Favorite> favoritesList = new List<Favorite>();

        public void AddFavorite(Favorite favorite)
        {
            favoritesList.Add(favorite);
            OnFavoritesUpdated?.Invoke(this);
            Save(true);
        }

        public bool IsFavorite(Object reference)
        {
            return favoritesList.Count(f => !f.reference.isBroken && f.reference.isSet && f.reference.asset == reference) > 0;
        }

        public void RemoveFavorite(Object reference)
        {
            favoritesList.RemoveAll(f => !f.reference.isBroken && f.reference.isSet && f.reference.asset == reference);
            OnFavoritesUpdated?.Invoke(this);
            Save(true);
        }

        public void RemoveAtIndex(int index)
        {
            if (index < 0 || index >= favoritesList.Count) 
                return;
            
            favoritesList.RemoveAt(index);
            OnFavoritesUpdated?.Invoke(this);
            Save(true);
        }

        public void OnFavoritesModified()
        {
            foreach (var favorite in favoritesList)
            {
                if (!favorite.reference.isBroken && favorite.reference.isSet)
                {
                    favorite.assetPath = AssetDatabase.GetAssetPath(favorite.reference.asset);
                }
            }
            Save(true);
        }
    }
}