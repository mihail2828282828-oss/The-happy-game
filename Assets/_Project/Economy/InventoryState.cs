using System;

namespace Front21.Economy
{
    [Serializable]
    public sealed class InventoryState
    {
        public string[] ItemIds = Array.Empty<string>();
        public int[] Counts = Array.Empty<int>();
    }
}
