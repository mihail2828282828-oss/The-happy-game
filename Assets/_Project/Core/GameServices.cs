using System;
using Front21.Content;
using Front21.Economy;
using Front21.Thermal;

namespace Front21.Core
{
    /// <summary>
    /// Единая точка доступа к сервисам. Инициализируется один раз из бутстрапа.
    /// </summary>
    public static class GameServices
    {
        public static IWallet Wallet { get; private set; }
        public static IThermalGuard Thermal { get; private set; }
        public static ISaveSystem Save { get; private set; }
        public static IInventory Inventory { get; private set; }
        public static IProgressProvider Progress { get; private set; }
        public static IPurchaseService Purchases { get; private set; }
        public static ShopCatalog Shop { get; private set; }
        public static FeaturedRotation Featured { get; private set; }

        public static bool IsReady { get; private set; }

        public static void Bootstrap(
            IWallet wallet,
            IThermalGuard thermal,
            ISaveSystem save,
            IInventory inventory,
            IProgressProvider progress,
            IPurchaseService purchases,
            ShopCatalog shop,
            FeaturedRotation featured)
        {
            if (IsReady) throw new InvalidOperationException("GameServices уже инициализирован.");

            Wallet = wallet ?? throw new ArgumentNullException(nameof(wallet));
            Thermal = thermal ?? throw new ArgumentNullException(nameof(thermal));
            Save = save ?? throw new ArgumentNullException(nameof(save));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Progress = progress ?? throw new ArgumentNullException(nameof(progress));
            Purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
            Shop = shop ?? throw new ArgumentNullException(nameof(shop));
            Featured = featured ?? throw new ArgumentNullException(nameof(featured));

            IsReady = true;
        }

        public static void Shutdown()
        {
            Wallet = null;
            Thermal = null;
            Save = null;
            Inventory = null;
            Progress = null;
            Purchases = null;
            Shop = null;
            Featured = null;
            IsReady = false;
        }
    }
}
