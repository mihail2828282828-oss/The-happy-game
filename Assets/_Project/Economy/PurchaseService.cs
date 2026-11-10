using System;
using Front21.Content;

namespace Front21.Economy
{
    public sealed class PurchaseService : IPurchaseService
    {
        readonly IWallet _wallet;
        readonly ShopCatalog _catalog;
        readonly IInventory _inventory;
        readonly IProgressProvider _progress;
        readonly FeaturedRotation _featured;

        public const int MaxLevelsAboveEquipment = 1;

        public event Action<ShopItemDef, long> Purchased;
        public event Action<ShopItemDef, long> Sold;

        public PurchaseService(
            IWallet wallet,
            ShopCatalog catalog,
            IInventory inventory,
            IProgressProvider progress,
            FeaturedRotation featured)
        {
            _wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _featured = featured;
        }

        public long PriceOf(ShopItemDef item, int level)
        {
            if (item == null || _catalog.Prices == null) return 0;

            long basePrice = _catalog.Prices.Compute(item.BasePrice, Math.Max(1, level));
            return _featured != null && _featured.IsFeatured(item.Id)
                ? FeaturedRotation.ApplyDiscount(basePrice)
                : basePrice;
        }

        public PurchaseResult TryBuy(string itemId, int quantity)
        {
            if (quantity <= 0) return PurchaseResult.Fail(PurchaseFailure.InvalidQuantity);

            ShopItemDef item = _catalog.Find(itemId);
            if (item == null) return PurchaseResult.Fail(PurchaseFailure.UnknownItem);

            if (!IsUnlocked(item)) return PurchaseResult.Fail(PurchaseFailure.LockedByLevel);

            int level = _inventory.Count(item.Id) + 1;
            if (level > item.MaxLevel + MaxLevelsAboveEquipment)
                return PurchaseResult.Fail(PurchaseFailure.LevelCapReached);

            if (item.Stackable && _inventory.Count(item.Id) + quantity > item.MaxStack)
                return PurchaseResult.Fail(PurchaseFailure.StackLimit);

            long unitPrice = PriceOf(item, level);
            long total = unitPrice * quantity;
            if (!_wallet.TrySpend(item.Currency, total))
                return PurchaseResult.Fail(PurchaseFailure.NotEnoughCurrency);

            int perStack = item.Stackable ? quantity : 1;
            for (int i = 0; i < perStack; i++)
            {
                if (_inventory.TryAdd(item.Id, 1, int.MaxValue)) continue;

                _wallet.Earn(item.Currency, unitPrice, EarnSource.Sell);
                return PurchaseResult.Fail(PurchaseFailure.StackLimit);
            }

            Purchased?.Invoke(item, total);
            return PurchaseResult.Ok(total);
        }

        public PurchaseResult TrySell(string itemId, int quantity)
        {
            if (quantity <= 0) return PurchaseResult.Fail(PurchaseFailure.InvalidQuantity);

            ShopItemDef item = _catalog.Find(itemId);
            if (item == null) return PurchaseResult.Fail(PurchaseFailure.UnknownItem);

            int owned = _inventory.Count(item.Id);
            if (owned <= 0) return PurchaseResult.Fail(PurchaseFailure.NotOwned);

            int toSell = Math.Min(quantity, owned);
            if (!_inventory.TryRemove(item.Id, toSell)) return PurchaseResult.Fail(PurchaseFailure.NotOwned);

            long unitPrice = PriceOf(item, Math.Max(1, owned));
            long total = Wallet.SellPrice(unitPrice) * toSell;

            _wallet.Earn(item.Currency, total, EarnSource.Sell);
            Sold?.Invoke(item, total);
            return PurchaseResult.Ok(total);
        }

        bool IsUnlocked(ShopItemDef item) => _progress.PlayerLevel >= item.UnlockLevel;
    }
}
