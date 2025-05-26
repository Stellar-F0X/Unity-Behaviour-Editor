namespace BehaviourSystem.BT
{
    public class FixedQueue<T> 
    {
        public FixedQueue(int capacity)
        {
            _capacity = capacity;
            _items = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }
        
        private readonly T[] _items;
        private readonly int _capacity;
        
        private int _head;
        private int _tail;
        private int _count;

        
        public int count
        {
            get { return _count; }
        }
        
        public T peek
        {
            get { return _items[_head]; }
        }
        
        public bool isFull
        {
            get { return _count == _capacity; }
        }

        
        public void Clear()
        {
            _head = 0;
            _tail = 0;
            _count = 0;
            
            for (int i = 0; i < _capacity; ++i)
            {
                _items[i] = default;
            }
        }

        public void Enqueue(T item)
        {
            if (isFull)
            {
                _head = (_head + 1) % _capacity;
                _count--;
            }

            _items[_tail] = item;
            _tail = (_tail + 1) % _capacity;
            _count++;
        }

        public T Dequeue()
        {
            T result = _items[_head];
            _items[_head] = default;
            _head = (_head + 1) % _capacity;
            _count--;

            return result;
        }
    }
}