using Front21.Content;
using Front21.Economy;
using UnityEngine;
using UnityEngine.UI;

namespace Front21.UI
{
    /// <summary>Строка товара: название, цена, покупка и продажа.</summary>
    public sealed class ShopRowView : MonoBehaviour
    {
        Text _title;
        Text _price;
        Button _buy;
        Button _sell;

        ShopItemDef _item;
        IPurchaseService _purchases;
        IWallet _wallet;
        IInventory _inventory;

        public static ShopRowView Create(Transform parent)
        {
            var go = new GameObject("Row", typeof(RectTransform), typeof(Image), typeof(ShopRowView));
            go.transform.SetParent(parent, false);

            var view = go.GetComponent<ShopRowView>();
            view.Build();
            return view;
        }

        void Build()
        {
            var rect = (RectTransform)transform;
            rect.sizeDelta = new Vector2(0f, 92f);

            _title = UiFactory.CreateText(rect, "Title", 24);
            RectTransform titleRect = _title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 0.5f);
            titleRect.anchorMax = new Vector2(0.55f, 1f);
            titleRect.offsetMin = new Vector2(12f, 6f);
            titleRect.offsetMax = new Vector2(-6f, -6f);

            _price = UiFactory.CreateText(rect, "Price", 22);
            RectTransform priceRect = _price.rectTransform;
            priceRect.anchorMin = new Vector2(0.55f, 0.5f);
            priceRect.anchorMax = new Vector2(0.72f, 1f);
            priceRect.offsetMin = Vector2.zero;
            priceRect.offsetMax = Vector2.zero;

            _buy = UiFactory.CreateButton(rect, "Buy", "Купить");
            PlaceButton((RectTransform)_buy.transform, 0.73f, 0.87f);

            _sell = UiFactory.CreateButton(rect, "Sell", "Продать");
            PlaceButton((RectTransform)_sell.transform, 0.88f, 0.99f);
        }

        static void PlaceButton(RectTransform rect, float from, float to)
        {
            rect.anchorMin = new Vector2(from, 0.15f);
            rect.anchorMax = new Vector2(to, 0.85f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void Bind(ShopItemDef item, IPurchaseService purchases, IWallet wallet, IInventory inventory)
        {
            _item = item;
            _purchases = purchases;
            _wallet = wallet;
            _inventory = inventory;

            _buy.onClick.RemoveAllListeners();
            _sell.onClick.RemoveAllListeners();
            _buy.onClick.AddListener(OnBuy);
            _sell.onClick.AddListener(OnSell);

            Refresh();
        }

        public void Refresh()
        {
            if (_item == null || _purchases == null) return;

            int owned = _inventory.Count(_item.Id);
            long price = _purchases.PriceOf(_item, owned + 1);

            _title.text = owned > 0 ? $"{_item.DisplayName} ×{owned}" : _item.DisplayName;
            _price.text = $"{price} {(_item.Currency == Currency.Coins ? "монет" : "крист.")}";

            _buy.interactable = _wallet.CanAfford(_item.Currency, price);
            _sell.interactable = owned > 0;
        }

        void OnBuy()
        {
            _purchases.TryBuy(_item.Id, 1);
            Refresh();
        }

        void OnSell()
        {
            _purchases.TrySell(_item.Id, 1);
            Refresh();
        }
    }
}
