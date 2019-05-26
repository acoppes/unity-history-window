using UnityEngine;

namespace Gemserk
{
    public static class SelectionHistoryWindowConstants
    {
        public static readonly string HistorySizePrefKey = "Gemserk.SelectionHistory.HistorySize";
        public static readonly string HistoryAutomaticRemoveDeletedPrefKey = "Gemserk.SelectionHistory.AutomaticRemoveDeleted";
        public static readonly string HistoryAllowDuplicatedEntriesPrefKey = "Gemserk.SelectionHistory.AllowDuplicatedEntries";
        public static readonly string HistoryShowHierarchyObjectsPrefKey = "Gemserk.SelectionHistory.ShowHierarchyObjects";
        public static readonly string HistoryShowProjectViewObjectsPrefKey = "Gemserk.SelectionHistory.ShowProjectViewObjects";
        public static readonly string HistoryFavoritesPrefKey = "Gemserk.SelectionHistory.Favorites";
        
        // this could be get from a stylesheet or something like that?
        public static readonly Color hierarchyElementColor = new Color(0.7f, 1.0f, 0.7f);
        public static readonly Color selectedElementColor = new Color(0.2f, 170.0f / 255.0f, 1.0f, 1.0f);
    }
}