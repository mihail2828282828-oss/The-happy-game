using System.Collections.Generic;
using Front21.Content;
using UnityEngine;

namespace Front21.Economy
{
    [CreateAssetMenu(menuName = "Front21/Каталог магазина", fileName = "ShopCatalog")]
    public sealed class ShopCatalog : ScriptableObject
    {
        [SerializeField] PriceTable _prices;
        [SerializeField] List<ShopItemDef> _items = new();

        readonly Dictionary<string, ShopItemDef> _byId = new();

        public PriceTable Prices => _prices;
        public IReadOnlyList<ShopItemDef> Items => _items;

        public ShopItemDef Find(string itemId)
        {
            EnsureIndex();
            if (string.IsNullOrEmpty(itemId)) return null;
            return _byId.TryGetValue(itemId, out ShopItemDef item) ? item : null;
        }

        public void EnsureIndex()
        {
            if (_byId.Count == _items.Count) return;

            _byId.Clear();
            for (int i = 0; i < _items.Count; i++)
            {
                ShopItemDef item = _items[i];
                if (item == null || string.IsNullOrEmpty(item.Id)) continue;
                _byId[item.Id] = item;
            }
        }

        public void ValidateAll()
        {
            if (_prices == null) Debug.LogError("[Shop] не назначена прайс-таблица");

            var seen = new HashSet<string>();
            for (int i = 0; i < _items.Count; i++)
            {
                ShopItemDef item = _items[i];
                if (item == null)
                {
                    Debug.LogError($"[Shop] пустая ячейка {i} в каталоге {name}");
                    continue;
                }

                item.Validate();
                if (!seen.Add(item.Id)) Debug.LogError($"[Shop] дубль id: {item.Id}");
            }
        }

        void OnEnable() => _byId.Clear();
    }
}
