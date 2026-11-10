using System;
using Front21.Content;

namespace Front21.Economy
{
    public interface IPurchaseService
    {
        long PriceOf(ShopItemDef item, int level);
        PurchaseResult TryBuy(string itemId, int quantity);
        PurchaseResult TrySell(string itemId, int quantity);

        event Action<ShopItemDef, long> Purchased;
        event Action<ShopItemDef, long> Sold;
    }
}
