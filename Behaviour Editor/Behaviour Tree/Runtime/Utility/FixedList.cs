using System.Collections.Generic;

namespace BehaviourSystem.BT
{
    public class FixedList<T>
    {
        public FixedList(int capacity)
        {
            _items = new T[capacity];
            _count = 0;
        }

        private readonly T[] _items;
        private int _count;

        
        public T this[int index]
        {
            get { return _items[index]; }
            set { _items[index] = value; }
        }

        public int count
        {
            get { return _count; }
        }

        

        public void Clear()
        {
            for (int i = 0; i < count; ++i)
            {
                _items[i] = default;
            }

            _count = 0;
        }


        public void Add(T item)
        {
            _items[_count++] = item;
        }


        public void Remove(T element)
        {
            for (int i = 0; i < _count; ++i)
            {
                if (EqualityComparer<T>.Default.Equals(_items[i], element))
                {
                    _items[i] = _items[_count - 1];
                    _items[_count - 1] = default;
                    --_count;
                    return;
                }
            }
        }
    }
}