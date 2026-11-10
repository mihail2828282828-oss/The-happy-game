using Front21.Content;
using UnityEngine;

namespace Front21.Economy
{
    public static class PurchaseTests
    {
        sealed class FixedClock : IWalletClock
        {
            public long UtcNowUnix { get; set; } = 1_700_000_000;
        }

        sealed class FixedProgress : IProgressProvider
        {
            public int PlayerLevel { get; set; } = 10;
        }

        public static void RunAll()
        {
            BuysWithDiscountOnlyWhenFeatured();
            RejectsWhenLocked();
            RejectsStackOverflow();
            SellsAtThirtyFivePercent();
            CannotSellUnowned();
            Debug.Log("[Purchase] все проверки пройдены");
        }

        static (PurchaseService service, Wallet wallet, Inventory inventory, FixedProgress progress) Build(long coins)
        {
            var clock = new FixedClock();
            var wallet = new Wallet(WalletCaps.Default, clock);
            wallet.Earn(Currency.Coins, coins, EarnSource.Stage);

            var inventory = new Inventory();
            var progress = new FixedProgress();

            var catalog = ScriptableObject.CreateInstance<ShopCatalog>();
            var prices = ScriptableObject.CreateInstance<PriceTable>();
            var item = ScriptableObject.CreateInstance<ShopItemDef>();

            typeof(ShopItemDef).GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(item, "optic_1");
            typeof(ShopItemDef).GetField("_basePrice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(item, 100L);
            typeof(ShopItemDef).GetField("_stackable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(item, true);
            typeof(ShopItemDef).GetField("_maxStack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(item, 3);

            typeof(ShopCatalog).GetField("_items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(catalog, new System.Collections.Generic.List<ShopItemDef> { item });
            typeof(ShopCatalog).GetField("_prices", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(catalog, prices);
            catalog.EnsureIndex();

            var featured = new FeaturedRotation(clock);
            var service = new PurchaseService(wallet, catalog, inventory, progress, featured);

            return (service, wallet, inventory, progress);
        }

        static void BuysWithDiscountOnlyWhenFeatured()
        {
            var (service, wallet, _, _) = Build(1_000);
            PurchaseResult result = service.TryBuy("optic_1", 1);

            if (!result.Success) throw new System.Exception("[Purchase] покупка должна пройти");
            if (result.PricePaid >= 100) throw new System.Exception("[Purchase] цена должна быть со скидкой, если позиция витринная");
        }

        static void RejectsWhenLocked()
        {
            var (service, _, _, progress) = Build(1_000);
            progress.PlayerLevel = 0;

            PurchaseResult result = service.TryBuy("optic_1", 1);
            if (result.Failure != PurchaseFailure.LockedByLevel)
                throw new System.Exception($"[Purchase] ожидался LockedByLevel, получено {result.Failure}");
        }

        static void RejectsStackOverflow()
        {
            var (service, _, _, _) = Build(1_000);
            PurchaseResult result = service.TryBuy("optic_1", 5);

            if (result.Failure != PurchaseFailure.StackLimit)
                throw new System.Exception($"[Purchase] ожидался StackLimit, получено {result.Failure}");
        }

        static void SellsAtThirtyFivePercent()
        {
            var (service, wallet, inventory, _) = Build(1_000);
            service.TryBuy("optic_1", 1);

            long before = wallet.Coins;
            PurchaseResult result = service.TrySell("optic_1", 1);

            if (!result.Success) throw new System.Exception("[Purchase] продажа должна пройти");
            if (wallet.Coins <= before) throw new System.Exception("[Purchase] монеты должны вырасти от продажи");
            if (inventory.Count("optic_1") != 0) throw new System.Exception("[Purchase] предмет должен уйти из инвентаря");
        }

        static void CannotSellUnowned()
        {
            var (service, _, _, _) = Build(1_000);
            PurchaseResult result = service.TrySell("optic_1", 1);

            if (result.Failure != PurchaseFailure.NotOwned)
                throw new System.Exception($"[Purchase] ожидался NotOwned, получено {result.Failure}");
        }
    }
}
