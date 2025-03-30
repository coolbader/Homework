using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace Homework
{
    /// <summary>
    /// 跳表节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class Node<T>
    {
        public Node<T>[] Next { get; }
        public T Value { get; }

        public Node(T value, int level)
        {
            if (level < 0) { throw new ArgumentException("Level must be >= 0!", nameof(level)); }
            Value = value;
            Next = new Node<T>[level];
        }

        public bool HasNextAtLevel(int level)
        {
            if (level < 0) { throw new ArgumentException("Level must be >= 0!", nameof(level)); }
            return level < Next.Length && Next[level] != null;
        }
    }

    /// <summary>
    /// 跳表实现
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class SkipList<T> : IEnumerable<T>, ISerializable
    {
        private readonly Node<T> _headNode = new Node<T>(default(T), 33);
        private readonly Random _randomGenerator = new Random();
        private int _levels = 1;
        private readonly IComparer<T> _comparer = Comparer<T>.Default;

        public SkipList(IComparer<T> comparer = null)
        {
            if (comparer != null)
            {
                _comparer = comparer;
            }
        }

        protected SkipList(SerializationInfo info, StreamingContext context)
        {
            _headNode = (Node<T>)info.GetValue("headNode", typeof(Node<T>));
            _levels = info.GetInt32("levels");
            _comparer = (IComparer<T>)info.GetValue("comparer", typeof(IComparer<T>));
        }

        public void Add(T value)
        {
            var level = 0;
            for (var r = _randomGenerator.Next(); (r & 1) == 1; r >>= 1)
            {
                level++;
                if (level == _levels)
                {
                    _levels++;
                    break;
                }
            }

            var addNode = new Node<T>(value, level + 1);
            var currentNode = _headNode;
            for (var currentLevel = _levels - 1; currentLevel >= 0; currentLevel--)
            {
                while (currentNode.HasNextAtLevel(currentLevel))
                {
                    if (_comparer.Compare(currentNode.Next[currentLevel].Value, value) == 1)
                    {
                        break;
                    }
                    currentNode = currentNode.Next[currentLevel];
                }

                if (currentLevel <= level)
                {
                    addNode.Next[currentLevel] = currentNode.Next[currentLevel];
                    currentNode.Next[currentLevel] = addNode;
                }
            }
        }

        public void AddRange(IEnumerable<T> values)
        {
            if (values == null) return;
            foreach (var value in values)
            {
                Add(value);
            }
        }

        public bool Contains(T value)
        {
            var currentNode = _headNode;
            for (var currentLevel = _levels - 1; currentLevel >= 0; currentLevel--)
            {
                while (currentNode.HasNextAtLevel(currentLevel))
                {
                    if (_comparer.Compare(currentNode.Next[currentLevel].Value, value) == 1) break;
                    if (_comparer.Compare(currentNode.Next[currentLevel].Value, value) == 0) return true;
                    currentNode = currentNode.Next[currentLevel];
                }
            }
            return false;
        }

        public void Update(T oldvalue, T newvalue){
            if (Remove(oldvalue))
            {
                Add(newvalue);
            }
        }

        public bool Remove(T value)
        {
            var currentNode = _headNode;

            var found = false;
            for (var currentLevel = _levels - 1; currentLevel >= 0; currentLevel--)
            {
                while (currentNode.HasNextAtLevel(currentLevel))
                {
                    if (_comparer.Compare(currentNode.Next[currentLevel].Value, value) == 0)
                    {
                        found = true;
                        currentNode.Next[currentLevel] = currentNode.Next[currentLevel].Next[currentLevel];
                        break;
                    }

                    if (_comparer.Compare(currentNode.Next[currentLevel].Value, value) == 1) { break; }

                    currentNode = currentNode.Next[currentLevel];
                }
            }

            return found;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            var currentNode = _headNode.Next[0];
            while (currentNode != null && currentNode.HasNextAtLevel(0))
            {
                yield return currentNode.Value;
                currentNode = currentNode.Next[0];
            }

            if (currentNode != null)
            {
                yield return currentNode.Value;
            }
        }

        public int GetIndex(T value)
        {
            var currentNode = _headNode.Next[0];
            int index = 0;
            while (currentNode != null)
            {
                if (_comparer.Compare(currentNode.Value, value) == 0)
                {
                    return index;
                }
                currentNode = currentNode.Next[0];
                index++;
            }
            return -1; 
        }

        public T GetByIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentException("Index must be non-negative!", nameof(index));
            }

            var currentNode = _headNode.Next[0];
            int currentIndex = 0;

            while (currentNode != null)
            {
                if (currentIndex == index)
                {
                    return currentNode.Value;
                }
                currentNode = currentNode.Next[0];
                currentIndex++;
            }

            throw new IndexOutOfRangeException("Index is out of range of the skip list.");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("headNode", _headNode);
            info.AddValue("levels", _levels);
            info.AddValue("comparer", _comparer);
        }

        public void BatchUpdateOrAdd(Queue<T> values)
        {
            if (values == null) return;
            foreach (var value in values)
            {
                if (Contains(value))
                {
                    Remove(value);
                    Add(value);
                }
                else
                {
                    Add(value);
                }
            }
        }
    }
}