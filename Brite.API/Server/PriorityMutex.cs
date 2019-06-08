using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Brite.Utility;

namespace Brite.API.Server
{
    class PriorityMutex<TPriority>
    {
        private readonly Mutex _mutex;
        private readonly List<KeyValuePair<object, TPriority>> _queue;

        private object _object;
        private TPriority _priority;

        public PriorityMutex()
        {
            _mutex = new Mutex();
            _queue = new List<KeyValuePair<object, TPriority>>();
        }

        public async Task LockAsync(object obj, TPriority priority)
        {
            if (_object == obj && _priority.Equals(priority))
                return;

            int queueCount;
            lock (_queue)
                queueCount = _queue.Count;

            if (queueCount == 0)
            {
                await _mutex.LockAsync();
                _object = obj;
                _priority = priority;

                return;
            }

            var pair = new KeyValuePair<object, TPriority>(obj, priority);
            lock (_queue)
                _queue.Add(pair);

            while (true)
            {
                await _mutex.LockAsync();
                lock (_queue)
                {
                    var priorityQueue = from entry in _queue orderby entry.Value descending select entry;
                    if (priorityQueue.First().Key == obj)
                    {
                        _queue.RemoveAt(0);
                        _object = obj;
                        _priority = priority;

                        return;
                    }

                    if (!_queue.Contains(pair))
                        throw new InvalidOperationException("Queue no longer contains a lock request by specified object");
                }

                _mutex.Unlock();
            }
        }

        public void Unlock(object obj)
        {
            if (_object == obj)
            {
                _mutex.Unlock();
                _object = null;
            }
        }

        public void RemoveLockRequest(object obj)
        {
            lock (_queue)
            {
                for (var i = 0; i < _queue.Count; i++)
                {
                    var pair = _queue[i];
                    if (pair.Key == obj)
                    {
                        _queue.Remove(pair);
                        i--;
                    }
                }
            }

            Unlock(obj);
        }

        public void Clear()
        {
            _queue.Clear();
            Unlock(_object);
        }
    }
}
