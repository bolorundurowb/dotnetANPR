using System;
using System.Collections.Generic;

namespace dotnetANPR.ImageAnalysis.Util
{
    public class CustomStack<T> : List<T>
    {
        public T Pop()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }
            var value = this[Count - 1];
            RemoveAt(Count - 1);
            return value;
        }

        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }
            var value = this[Count - 1];
            return value;
        }

        public void Push(T item)
        {
            Add(item);
        }
    }
}
