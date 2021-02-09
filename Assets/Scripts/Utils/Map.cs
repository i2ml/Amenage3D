using System.Collections.Generic;

namespace ErgoShop.Utils
{
    /// <summary>
    ///     NOT USED YET
    ///     Class to have a map
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class Map<T1, T2>
    {
        private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
        private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

        public Map()
        {
            Forward = new Indexer<T1, T2>(_forward);
            Reverse = new Indexer<T2, T1>(_reverse);
        }

        public Indexer<T1, T2> Forward { get; }
        public Indexer<T2, T1> Reverse { get; }

        public void Add(T1 t1, T2 t2)
        {
            _forward.Add(t1, t2);
            _reverse.Add(t2, t1);
        }

        public void Remove(T1 t1)
        {
            var t2 = _forward[t1];
            _forward.Remove(t1);
            _reverse.Remove(t2);
        }

        public void Remove(T2 t2)
        {
            var t1 = _reverse[t2];
            _forward.Remove(t1);
            _reverse.Remove(t2);
        }

        public class Indexer<T3, T4>
        {
            private readonly Dictionary<T3, T4> _dictionary;

            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }

            public T4 this[T3 index]
            {
                get => _dictionary[index];
                set => _dictionary[index] = value;
            }
        }
    }
}