using System.Collections.Generic;
using Front21.Content;
using Front21.Core;
using Front21.Economy;
using UnityEngine;

namespace Front21.UI
{
    /// <summary>Собирает список товаров выбранной категории. Вешать на панель магазина.</summary>
    public sealed class ShopScreenPresenter : MonoBehaviour
    {
        [SerializeField] Transform _content;
        [SerializeField] ShopCategory _category = ShopCategory.Arsenal;

        readonly List<ShopRowView> _rows = new();

        ShopCatalog _catalog;
        IPurchaseService _purchases;
        IWallet _wallet;
        IInventory _inventory;

        void Start()
        {
            if (!GameServices.IsReady)
            {
                Debug.LogWarning("[Shop] сервисы не готовы, витрина не собрана");
                enabled = false;
                return;
            }

            _wallet = GameServices.Wallet;
            _inventory = GameServices.Inventory;
            _purchases = GameServices.Purchases;
            _catalog = GameServices.Shop;

            if (_catalog != null && _catalog.Prices != null)
                GameServices.Featured?.Refresh(_catalog.Items);

            Build();
        }

        void Build()
        {
            Clear();
            if (_catalog == null || _content == null) return;

            IReadOnlyList<ShopItemDef> items = _catalog.Items;
            for (int i = 0; i < items.Count; i++)
            {
                ShopItemDef item = items[i];
                if (item == null || item.Category != _category) continue;

                ShopRowView row = ShopRowView.Create(_content);
                row.Bind(item, _purchases, _wallet, _inventory);
                _rows.Add(row);
            }
        }

        void Clear()
        {
            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i] != null) Destroy(_rows[i].gameObject);

            _rows.Clear();
        }

        void OnDestroy() => Clear();
    }
}
