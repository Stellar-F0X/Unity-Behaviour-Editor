namespace BehaviourSystem.BT
{
    public class FixedStack<T>
    {
        public FixedStack(int capacity)
        {
            _items = new T[capacity];
            _capacity = capacity;
            _count = 0;
        }

        private readonly T[] _items;
        private readonly int _capacity;
        private int _count;


        public int count
        {
            get { return _count; }
        }

        public bool isEmpty
        {
            get { return _count == 0; }
        }
        
        public T peek
        {
            get { return _items[_count - 1]; }
        }


        public void Push(T item)
        {
            _items[_count++] = item;
        }


        public T Pop()
        {
            return _items[--_count];
        }
    }
}