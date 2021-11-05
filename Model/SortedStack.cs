using System;
using System.Collections.Generic;

namespace Model
{
    public class SortedStack<T>
    {
        private readonly List<T> data;
        private readonly List<double> dataValues;

        public SortedStack(int capacity = 4)
        {
            data = new List<T>(capacity);
            dataValues = new List<double>(capacity);
        }

        public T[] ToArray() => data.ToArray();

        public bool IsEmpty => data.Count == 0;

        public int Count => data.Count;

        public bool Remove(T item)
        {
            var index = data.IndexOf(item);
            if (index == -1)
                return false;

            data.RemoveAt(index);
            dataValues.RemoveAt(index);
            
            return true;
        }

        public void Push(T item, double value)
        {
            var position = dataValues.BinarySearch(value);
            
            if (position < 0)
                position = ~position;

            this.data.Insert(position, item);
            dataValues.Insert(position, value);
        }

        public void Update(T item, double value)
        {
            if (!Remove(item))
                throw new ArgumentException("No node");

            Push(item, value);
        }

        public T Pop()
        {
            if (data.Count == 0)
                return default;

            var item = data[0];
            data.RemoveAt(0);
            dataValues.RemoveAt(0);

            return item;
        }
    }
}