using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
    public class FavoritesAsset : ScriptableObject
    {
        [Serializable]
        public class Favorite
        {
            public Object reference;
        }
    
        public event Action<FavoritesAsset> OnFavoritesUpdated;

        public List<Favorite> favoritesList = new List<Favorite>();

        public void AddFavorite(Favorite favorite)
        {
            favoritesList.Add(favorite);
            OnFavoritesUpdated?.Invoke(this);
        }

        public void ClearUnreferenced()
        {
            favoritesList.RemoveAll(f => f.reference == null);
        }

        public bool IsFavorite(Object reference)
        {
            return favoritesList.Count(f => f.reference == reference) > 0;
        }

        public void RemoveFavorite(Object reference)
        {
            favoritesList.RemoveAll(f => f.reference == reference);
            OnFavoritesUpdated?.Invoke(this);
        }
    }
}