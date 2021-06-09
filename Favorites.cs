using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
    [CreateAssetMenu(menuName = "Gemserk/Favorites Asset")]
    public class Favorites : ScriptableObject
    {
        [Serializable]
        public class Favorite
        {
            public Object reference;
        }
    
        public event Action<Favorites> OnFavoritesUpdated;

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
    }
}