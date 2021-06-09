public class FavoritesSingleton
{
    private static Favorites _favoritesInstance;
    
    public static Favorites Instance
    {
        get
        {
            if (_favoritesInstance == null)
            {
                _favoritesInstance = new Favorites();
                _favoritesInstance.OnFavoritesUpdated += OnFavoritesUpdated;
                // read from disk and initialize...
            }
            return _favoritesInstance;
        }
    }

    private static void OnFavoritesUpdated(Favorites favorites)
    {
        // store asset in disk...
    }
}