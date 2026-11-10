using System.Collections.Generic;

namespace Front21.Core
{
    public sealed class MemorySaveSystem : ISaveSystem
    {
        readonly Dictionary<string, object> _store = new();

        public bool TryLoad<T>(string key, out T value) where T : class
        {
            if (_store.TryGetValue(key, out object raw) && raw is T typed)
            {
                value = typed;
                return true;
            }

            value = null;
            return false;
        }

        public void Store<T>(string key, T value) where T : class
        {
            if (string.IsNullOrEmpty(key)) return;
            _store[key] = value;
        }

        public void Flush() { }
        public void Wipe() => _store.Clear();
    }
}
