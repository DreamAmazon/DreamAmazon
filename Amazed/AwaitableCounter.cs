using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DreamAmazon
{
    public class AwaitableCounter<T> where T : class 
    {
        private readonly int _treshold;
        private readonly ConcurrentDictionary<T, int> _cache = new ConcurrentDictionary<T, int>();
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public AwaitableCounter(int treshold)
        {
            _treshold = treshold;
        }

        public void Init(T item)
        {
            if (!_cache.ContainsKey(item))
            {
                _cache.TryAdd(item, 0);
            }
        }

        public void Increment(T item)
        {
            if (_cache.ContainsKey(item) && _cache[item] < _treshold)
            {
                _cache[item]++;
            }
            else if (_cache.ContainsKey(item) && _cache[item] == 3)
            {
                int i;
                _cache.TryRemove(item, out i);
                _manualResetEvent.Set();
            }
        }

        public bool Wait(T item, TimeSpan timeSpan)
        {
            if (!_manualResetEvent.WaitOne(timeSpan))
            {
                return false;
            }

            return true;
        }
    }
}