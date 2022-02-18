using System;
using System.Collections.Generic;
using System.Linq;

namespace MP3DL.Media
{
    public class NavigatableList<T> : List<T>
    {
        public NavigatableList()
        {
            ReaderHead = 0;
        }
        public int ReaderHead { get; set; }
        public event EventHandler NextIsNull;
        public event EventHandler PrevIsZero;
        public bool IsNextNull()
        {
            if (ReaderHead < Count - 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool IsPrevZero()
        {
            if (ReaderHead > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public void Next()
        {
            if (ReaderHead < this.Count() - 1)
            {
                ReaderHead++;
            }
            else
            {
                OnNextIsNull();
            }
        }
        public void Prev()
        {
            if (ReaderHead > 0)
            {
                ReaderHead--;
            }
            else
            {
                OnPrevIsZero();
            }
        }
        public T GetCurrent()
        {
            return base[ReaderHead];
        }
        public new void Add(T item)
        {
            base.Add(item);
            ReaderHead = this.Count - 1;
        }
        public new void Remove(T item)
        {
            int index = 0;
            for (int i = 0; i < Count; i++)
            {
                if (base[i].Equals(item))
                {
                    index = i;
                    break;
                }
            }
            if (ReaderHead >= index)
            {
                Prev();
            }
            base.Remove(item);
        }
        public void RemoveAtReaderHead()
        {
            base.RemoveAt(ReaderHead);
        }
        public void InsertAtReaderHead(T item)
        {
            base.Insert(ReaderHead, item);
        }
        protected virtual void OnNextIsNull()
        {
            NextIsNull?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnPrevIsZero()
        {
            PrevIsZero?.Invoke(this, EventArgs.Empty);
        }
    }
}
