using System;
using System.Collections.Generic;

namespace Front21.Economy
{
    public sealed class Inventory : IInventory
    {
        readonly Dictionary<string, int> _items = new();

        public IReadOnlyCollection<string> ItemIds => _items.Keys;

        public int Count(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) return 0;
            return _items.TryGetValue(itemId, out int count) ? count : 0;
        }

        public bool TryAdd(string itemId, int amount, int maxStack)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;

            int current = Count(itemId);
            if (maxStack > 0 && current + amount > maxStack) return false;

            _items[itemId] = current + amount;
            return true;
        }

        public bool TryRemove(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0) return false;

            int current = Count(itemId);
            if (current < amount) return false;

            int left = current - amount;
            if (left == 0) _items.Remove(itemId);
            else _items[itemId] = left;

            return true;
        }

        public InventoryState Snapshot()
        {
            var state = new InventoryState
            {
                ItemIds = new string[_items.Count],
                Counts = new int[_items.Count]
            };

            int i = 0;
            foreach (KeyValuePair<string, int> pair in _items)
            {
                state.ItemIds[i] = pair.Key;
                state.Counts[i] = pair.Value;
                i++;
            }

            return state;
        }

        public void Restore(InventoryState state)
        {
            _items.Clear();
            if (state?.ItemIds == null || state.Counts == null) return;

            int limit = Math.Min(state.ItemIds.Length, state.Counts.Length);
            for (int i = 0; i < limit; i++)
            {
                string id = state.ItemIds[i];
                if (string.IsNullOrEmpty(id) || state.Counts[i] <= 0) continue;
                _items[id] = state.Counts[i];
            }
        }
    }
}
