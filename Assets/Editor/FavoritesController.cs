
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public class FavoritesController
{
    private static FavoritesController favoritesController;
    
    public static FavoritesController Instance
    {
        get
        {
            if (favoritesController == null)
            {
                favoritesController = new FavoritesController();
                // read from disk and initialize...
            }
            return favoritesController;
        }
    }

    private readonly Favorites favorites = new Favorites();

    public event Action<Favorites> OnFavoritesUpdated;
    
    private FavoritesController()
    {
        
    }

    public void AddFavorite(Favorites.Favorite favorite)
    {
        favorites.favoritesList.Add(favorite);
        OnFavoritesUpdated?.Invoke(favorites);
        
        // update asset
    }

    public Favorites GetFavorites()
    {
        return favorites;
        
    }
}

public class Favorites
{
    public class Favorite
    {
        public Object reference;
    }

    public List<Favorite> favoritesList = new List<Favorite>();
}