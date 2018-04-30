using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace PRTrackerUI.Common
{
    public class AsyncCache<TKey, TValue>
    {
        private readonly Func<TKey, Task<TValue>> valueFactory;
        private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> map;

        public AsyncCache(Func<TKey, Task<TValue>> valueFactory)
        {
            this.valueFactory = valueFactory ?? throw new ArgumentNullException(nameof(valueFactory));
            this.map = new ConcurrentDictionary<TKey, Lazy<Task<TValue>>>();
        }

        public Task<TValue> this[TKey key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return this.map.GetOrAdd(key, toAdd => new Lazy<Task<TValue>>(() => this.valueFactory(toAdd))).Value;
            }
        }

        public bool ContainsKey(TKey key)
        {
            return this.map.ContainsKey(key);
        }
    }
}
