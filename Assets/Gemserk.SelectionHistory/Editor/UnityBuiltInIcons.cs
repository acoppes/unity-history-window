namespace Gemserk
{
    public static class UnityBuiltInIcons
    {
        public const string pickObjectIconName = "d_pick";
        public const string favoriteWindowIconName = "d_Favorite Icon";
        
        #if UNITY_2022_3_OR_NEWER
        public const string favoriteIconName = "d_Favorite_colored";
        public const string favoriteEmptyIconName = "d_Favorite icon";
        #else 
        public const string favoriteIconName = "d_Toolbar Minus";
        public const string favoriteEmptyIconName = "d_Toolbar Plus";
        #endif

        
        public const string removeIconName = "d_Toolbar Minus";
        public const string tagIconName = "AssetLabelIcon";
        public const string searchIconName = "d_Search Icon";
        public const string eyeViewToolIconName = "d_ViewToolOrbit";
        public const string refreshIconName = "TreeEditor.Refresh";

        public const string openAssetIconName = "d_FolderOpened Icon";
        public const string clearSearchToolbarIconName = "d_clear";
        
    }
}