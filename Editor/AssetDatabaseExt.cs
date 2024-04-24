using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Gemserk
{
    public static class AssetDatabaseExt
    {
        public static string GetSearchFilter(Type type)
        {
            return $"t:{type.Name}";
        }
        
        public static List<Object> FindAssets(Type type, string text, string[] folders = null)
        {
            var searchFilter = GetSearchFilter(type);
            
            if (!string.IsNullOrEmpty(text))
            {
                searchFilter += $" {text}";
            }
            
            var guids = AssetDatabase.FindAssets(searchFilter, folders);
            return guids.Select(g => AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(g), type)).ToList();
        }
    }
}