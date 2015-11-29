using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Asgard.Core.Collections
{
    public class LinkedListNode<T>
    {
        public LinkedListNode<T> Next { get; set; }
        public LinkedListNode<T> Previous { get; set; }
        public T Value { get; set; }

        public LinkedListNode(T item)
        {
            Value = item;
        }
    }

    public class LinkedList<T> : IEnumerable<LinkedListNode<T>>
    {
        private LinkedListNode<T> _head;
        private LinkedListNode<T> _tail;

        public IEnumerator<LinkedListNode<T>> GetEnumerator()
        {
            var current = _head;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            var current = _head;
            while (current != null)
            {
                yield return current;
                current = current.Next;
            }
        }

        public LinkedListNode<T> First
        {
            get { return _head; }
        }

        public LinkedListNode<T> Last
        {
            get { return _tail; }
        }

        public LinkedListNode<T> AddToHead(T value)
        {
            if (_head == null) return createNewHead(value);
            if (_tail == null) _tail = findTail();

            return InsertBeforeNode(value, _head);
        }

        public LinkedListNode<T> AddToTail(T value)
        {
            if (_head == null) return createNewHead(value);
            if (_tail == null) _tail = findTail();

            return InsertAfterNode(value, _tail);
        }

        public LinkedListNode<T> InsertAfterNode(T value, LinkedListNode<T> node)
        {
            var n = node.Next;
            var newNode = new LinkedListNode<T>(value);
            newNode.Next = n;
            newNode.Previous = node;
            node.Next = newNode;

            if (node == _tail)
                _tail = newNode;
            return newNode;
        }

        public LinkedListNode<T> InsertBeforeNode(T value, LinkedListNode<T> node)
        {
            var n = node.Previous;
            var newNode = new LinkedListNode<T>(value);
            newNode.Next = node;
            newNode.Previous = n;
            node.Previous = newNode;

            if (node == _head)
                _head = newNode;

            return newNode;
        }

        public void TruncateTo(LinkedListNode<T> node)
        {
            _head = node;
            node.Previous = null;
        }

        private LinkedListNode<T> findTail()
        {
            if (_head == null) return null;
            LinkedListNode<T> current = _head;
            while (current.Next != null)
            {
                current = current.Next;
            }
            return current;
        }

        private LinkedListNode<T> createNewHead(T value)
        {
            LinkedListNode<T> newNode = new LinkedListNode<T>(value);
            _head = newNode;
            _tail = newNode;
            return newNode;
        }
    }
}
