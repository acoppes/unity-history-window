using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class Favorites
{
    public class Favorite
    {
        public Object reference;
    }
    
    public event Action<Favorites> OnFavoritesUpdated;

    public readonly List<Favorite> favoritesList = new List<Favorite>();
    
    public void AddFavorite(Favorite favorite)
    {
        favoritesList.Add(favorite);
        OnFavoritesUpdated?.Invoke(this);
        // update asset
    }
    
}