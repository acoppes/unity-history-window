using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gemserk
{
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
            
            public Object reference;

            public string sceneName;
            public string scenePath;
            
            public string globalObjectId;

            private string unreferencedObjectName;

            public bool isSceneInstance => !string.IsNullOrEmpty(sceneName);

            public bool isAsset => !isSceneInstance;

            public string name
            {
                get
                {
                    if (reference != null)
                    {
                        return reference.name;
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
                    if (go.scene != null)
                    {
                        sceneName = go.scene.name;
                        scenePath = go.scene.path;
                    }
                }
            }

            public bool Equals(Entry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(reference, other.reference);
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
                return (reference != null ? reference.GetHashCode() : 0);
            }

            public State GetReferenceState()
            {
                if (reference != null)
                    return State.Referenced;
                if (!string.IsNullOrEmpty(globalObjectId))
                    return State.ReferenceUnloaded;
                return State.ReferenceDestroyed;
            }
        }

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

        private int historySize = 10;

        public List<Entry> History => _history;
        
        public event Action<SelectionHistory> OnNewEntryAdded;

        public int HistorySize
        {
            get => historySize;
            set => historySize = value;
        }

        public bool IsSelected(int index)
        {
            return index == currentSelectionIndex;
        }

        public bool IsSelected(Object obj)
        {
            if (currentSelection == null)
                return false;
            if (currentSelection.GetReferenceState() == Entry.State.Referenced)
                return currentSelection.reference.Equals(obj);
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
            return currentSelection.reference;
        }

        public void UpdateSelection(Object selection)
        {
            if (selection == null)
                return;

            var lastSelectedObject = _history.Count > 0 ? _history.Last() : null;

            var isLastSelected = lastSelectedObject != null && lastSelectedObject.reference == selection;
            var isCurrentSelection = currentSelection != null && currentSelection.reference == selection;
            
            if (!isLastSelected && !isCurrentSelection)
            {
                _history.Add(new Entry(selection));
                currentSelectionIndex = _history.Count - 1;
                
                OnNewEntryAdded?.Invoke(this);
            }

            if (_history.Count > historySize)
            {
                _history.RemoveRange(0, _history.Count - historySize);
                //			_history.RemoveAt(0);
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
            currentSelectionIndex = _history.FindIndex(e => e.reference.Equals(obj));
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