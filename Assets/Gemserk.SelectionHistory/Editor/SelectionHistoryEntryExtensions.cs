namespace Gemserk
{
    public static class SelectionHistoryEntryExtensions
    {
        public static string GetName(this SelectionHistory.Entry e, bool appendScene)
        {
            if (appendScene && e.isSceneInstance)
            {
                return $"{e.sceneName}/{e.name}";
            }
            return e.name;
        }
    }
}