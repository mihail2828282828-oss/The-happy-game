using System;
using System.Collections.Generic;

namespace Front21.Economy
{
    public interface IInventory
    {
        int Count(string itemId);
        IReadOnlyCollection<string> ItemIds { get; }

        bool TryAdd(string itemId, int amount, int maxStack);
        bool TryRemove(string itemId, int amount);

        InventoryState Snapshot();
        void Restore(InventoryState state);
    }
}
