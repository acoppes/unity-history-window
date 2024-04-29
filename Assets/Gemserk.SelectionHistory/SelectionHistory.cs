using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
    public static class SelectionHistoryUtils
    {
        public static bool IsSceneObject(Object reference)
        {
            if (reference is GameObject go)
            {
                return go.scene.isLoaded;
            }
            return false;
        }
    }
    
    [Serializable]
    public class SelectionHistory
    {
        [Serializable]
        public class Entry : IEquatable<Entry>
        {
            public enum State
            {
                Referenced = 0,
                ReferenceDestroyed = 1,
                ReferenceUnloaded = 2
            }

            // public State state = State.Referenced;

            [SerializeField]
            public Object reference;

            [NonSerialized]
            private Object hierarchyObjectReference;

            public Object Reference
            {
                get
                {
                    if (isAsset)
                        return reference;

#if UNITY_EDITOR
                    if (hierarchyObjectReference == null && UnityEditor.GlobalObjectId.TryParse(globalObjectId, out var id))
                    {
                        hierarchyObjectReference = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id);
                    }
#endif

                    return hierarchyObjectReference;
                }
            }

            public string sceneName;
            public string scenePath;
            
            public string globalObjectId;

            private string unreferencedObjectName;

            public bool isSceneInstance => !string.IsNullOrEmpty(sceneName);

            public bool isUnloadedHierarchyObject => isSceneInstance && GetReferenceState() == State.ReferenceUnloaded;

            public bool isAsset => !isSceneInstance;

            public bool isReferenced => GetReferenceState() == State.Referenced;

            public string name
            {
                get
                {
                    if (Reference != null)
                    {
                        return Reference.name;
                    }
                    return unreferencedObjectName;
                }   
            }

            public Entry(Object reference)
            {
                this.reference = reference;
                unreferencedObjectName = reference.name;

                if (reference is GameObject go)
                {
                    if (!string.IsNullOrEmpty(go.scene.name))
                    {
                        this.reference = null;
                        hierarchyObjectReference = reference;
                        
                        sceneName = go.scene.name;
                        scenePath = go.scene.path;
                        
                        #if UNITY_EDITOR
                        globalObjectId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(reference).ToString();
                        #endif
                    }
                }
            }

            public bool Equals(Entry other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }
                
                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                if (Reference == null && other.Reference == null)
                {
                    return string.Equals(scenePath, other.scenePath) &&
                           string.Equals(globalObjectId, other.globalObjectId);
                }
                
                return Equals(Reference, other.Reference);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((Entry) obj);
            }

            public override int GetHashCode()
            {
                return (Reference != null ? Reference.GetHashCode() : 0);
            }

            public State GetReferenceState()
            {
                if (Reference != null)
                    return State.Referenced;
                if (!string.IsNullOrEmpty(globalObjectId))
                    return State.ReferenceUnloaded;
                return State.ReferenceDestroyed;
            }
        }
        
        public int historySize = 200;

        [SerializeField] 
        private List<Entry> _history = new List<Entry>(100);

        private int currentSelectionIndex;

        private Entry currentSelection
        {
            get
            {
                if (currentSelectionIndex >= 0 && currentSelectionIndex < _history.Count)
                {
                    return _history[currentSelectionIndex];
                }
                return null;
            }
        }
        
        public List<Entry> History => _history;
        
        public event Action<SelectionHistory> OnNewEntryAdded;

        public bool IsSelected(int index)
        {
            return index == currentSelectionIndex;
        }

        public int GetSelectedIndex()
        {
            return currentSelectionIndex;
        }

        public bool IsSelected(Object obj)
        {
            if (currentSelection == null)
                return false;
            if (currentSelection.GetReferenceState() == Entry.State.Referenced)
                return currentSelection.Reference.Equals(obj);
            return false;
        }

        public void Clear()
        {
            _history.Clear();
        }

        public int GetHistoryCount()
        {
            return _history.Count;
        }

        public Object GetSelection()
        {
            return currentSelection.Reference;
        }

        public void UpdateSelection(Object selection)
        {
            if (selection == null)
                return;

            var lastSelectedObject = _history.Count > 0 ? _history.Last() : null;

            var isLastSelected = lastSelectedObject != null && lastSelectedObject.Reference == selection;
            var isCurrentSelection = currentSelection != null && currentSelection.Reference == selection;
            
            if (!isLastSelected && !isCurrentSelection)
            {
                _history.Add(new Entry(selection));
                currentSelectionIndex = _history.Count - 1;
            }

            if (_history.Count > historySize)
            {
                _history.RemoveRange(0, _history.Count - historySize);
                //			_history.RemoveAt(0);
            }
            
            if (!isLastSelected && !isCurrentSelection)
            {
                OnNewEntryAdded?.Invoke(this);
            }
        }

        public void Previous()
        {
            if (_history.Count == 0)
                return;

            currentSelectionIndex--;
            if (currentSelectionIndex < 0)
                currentSelectionIndex = 0;
        }

        public void Next()
        {
            if (_history.Count == 0)
                return;

            currentSelectionIndex++;
            if (currentSelectionIndex >= _history.Count)
                currentSelectionIndex = _history.Count - 1;
        }

        public void SetSelection(Object obj)
        {
            currentSelectionIndex = _history.FindIndex(e => obj.Equals(e.Reference));
        }

        public void RemoveEntries(Entry.State state)
        {
            var deletedCount = _history.Count(e => e.GetReferenceState() == state);

            var currentSelectionDestroyed = currentSelection == null || currentSelection.GetReferenceState() == state;
            
            _history.RemoveAll(e => e.GetReferenceState() == state);

            currentSelectionIndex -= deletedCount;

            if (currentSelectionIndex < 0)
                currentSelectionIndex = 0;

            if (currentSelectionDestroyed)
                currentSelectionIndex = -1;
        }

        public void RemoveDuplicated()
        {
            var tempList = new List<Entry>(_history);

            foreach (var item in tempList)
            {
                var itemFirstIndex = _history.IndexOf(item);
                var itemLastIndex = _history.LastIndexOf(item);

                while (itemFirstIndex != itemLastIndex)
                {
                    _history.RemoveAt(itemFirstIndex);

                    itemFirstIndex = _history.IndexOf(item);
                    itemLastIndex = _history.LastIndexOf(item);
                }
            }

            if (currentSelectionIndex >= _history.Count)
                currentSelectionIndex = _history.Count - 1;
        
        }
    }
}