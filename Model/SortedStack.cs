using System.Collections.Generic;

namespace Model
{
    public class SortedStack<T>
    {
        private readonly List<T> data;
        private readonly List<double> dataOrder;

        public SortedStack(int capacity = 4)
        {
            data = new List<T>(capacity);
            dataOrder = new List<double>(capacity);
        }

        public bool IsEmpty => data.Count == 0;

        public int Count => data.Count;

        public bool Remove(T item)
        {
            var index = data.IndexOf(item);
            if (index == -1)
                return false;

            data.RemoveAt(index);
            dataOrder.RemoveAt(index);
            
            return true;
        }

        public void Push(T item, double weight)
        {
            int position = dataOrder.BinarySearch(weight);
            if (position < 0)
            {
                position = ~position;
            }

            data.Insert(position, item);
            dataOrder.Insert(position, weight);
            //Debug.WriteLine($"{weight}");
        }

        public void Update(T item, double weight)
        {
            Remove(item);
            Push(item, weight);
        }

        public T Pop()
        {
            if (data.Count == 0)
                return default;

            var item = data[0];
            data.RemoveAt(0);
            dataOrder.RemoveAt(0);

            return item;
        }
    }
}