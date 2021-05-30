﻿using System;
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
            public Object reference;

            private string _name;

            public string name => reference == null ? _name : reference.name;

            public Entry(Object reference)
            {
                this.reference = reference;
                _name = reference.name;
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

            public bool ReferenceIsNull()
            {
                return reference == null;
            }
        }

        [SerializeField] 
        private List<Entry> _history = new List<Entry>(100);

        [SerializeField] 
        private List<Entry> _favorites = new List<Entry>(100);

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

        public List<Entry> Favorites => _favorites;

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
            return currentSelection.reference.Equals(obj);
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
            }

            // currentSelection = selection;

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

        public void ClearDeleted()
        {
            var deletedCount = _history.Count(e => e.ReferenceIsNull());

            var currentSelectionWasNull = currentSelection == null ? true : currentSelection.ReferenceIsNull();
            
            _history.RemoveAll(e => e.ReferenceIsNull());
            _favorites.RemoveAll(f => f.ReferenceIsNull());

            currentSelectionIndex -= deletedCount;

            if (currentSelectionIndex < 0)
                currentSelectionIndex = 0;

            if (currentSelectionWasNull)
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

        public bool IsFavorite(Object obj)
        {
            return _favorites.Find(f => f.reference.Equals(obj)) != null;
        }

        public void ToggleFavorite(Object obj)
        {
            if (IsFavorite(obj))
            {
                _favorites.RemoveAll(f => f.reference.Equals(obj));
            }
            else
            {
                _favorites.Add(new Entry(obj));
            }
        }
        

    }
}