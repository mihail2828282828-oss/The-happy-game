using Front21.Content;
using Front21.Economy;
using Front21.Thermal;
using UnityEngine;

namespace Front21.Core
{
    /// <summary>
    /// Точка входа в сцену: собирает сервисы, восстанавливает состояние, гоняет
    /// термоконтроль и сохраняет прогресс при паузе и уничтожении.
    /// </summary>
    public sealed class DeviceBootstrapper : MonoBehaviour
    {
        const string WalletKey = "wallet";
        const string InventoryKey = "inventory";
        const string ProgressKey = "progress";

        [SerializeField] bool _coolModeOnStart;

        [Tooltip("Если пусто, каталог создаётся пустым в рантайме.")]
        [SerializeField] ShopCatalog _shopCatalog;

        ThermalGuard _thermal;
        MemorySaveSystem _save;
        Wallet _wallet;
        Inventory _inventory;
        PlayerProgress _progress;
        FeaturedRotation _featured;
        PurchaseService _purchases;
        ShopCatalog _catalog;
        bool _shutdown;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);

#if UNITY_ANDROID && !UNITY_EDITOR
            IThermalSensor sensor = new AndroidBatteryThermalSensor();
#else
            IThermalSensor sensor = new NullThermalSensor();
#endif
            _thermal = new ThermalGuard(sensor);
            _save = new MemorySaveSystem();

            _wallet = new Wallet();
            if (_save.TryLoad(WalletKey, out WalletState walletState)) _wallet.Restore(walletState);

            _inventory = new Inventory();
            if (_save.TryLoad(InventoryKey, out InventoryState inventoryState)) _inventory.Restore(inventoryState);

            _progress = new PlayerProgress();
            if (_save.TryLoad(ProgressKey, out PlayerProgressState progressState)) _progress.Restore(progressState);

            _catalog = _shopCatalog != null ? _shopCatalog : ScriptableObject.CreateInstance<ShopCatalog>();
            _catalog.EnsureIndex();

            _featured = new FeaturedRotation(new SystemWalletClock());
            if (_catalog.Prices != null) _featured.Refresh(_catalog.Items);

            _purchases = new PurchaseService(_wallet, _catalog, _inventory, _progress, _featured);

            GameServices.Bootstrap(_wallet, _thermal, _save, _inventory, _progress, _purchases, _catalog, _featured);

            if (_coolModeOnStart) _thermal.SetCoolMode(true);

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        void Update()
        {
            if (GameServices.Thermal != null)
                GameServices.Thermal.Tick(Time.unscaledDeltaTime);
        }

        void OnApplicationPause(bool paused)
        {
            if (!paused || _shutdown) return;

            PersistAll();
            GameServices.Save?.Flush();
        }

        void OnApplicationQuit()
        {
            if (_shutdown) return;

            PersistAll();
            GameServices.Save?.Flush();
        }

        void OnDestroy() => Shutdown();

        void Shutdown()
        {
            if (_shutdown) return;
            _shutdown = true;

            PersistAll();
            GameServices.Save?.Flush();

            _thermal?.Dispose();
            GameServices.Shutdown();

            _wallet = null;
            _inventory = null;
            _progress = null;
            _purchases = null;
            _thermal = null;
            _save = null;
        }

        void PersistAll()
        {
            if (_save == null) return;

            if (_wallet != null) _save.Store(WalletKey, _wallet.Snapshot());
            if (_inventory != null) _save.Store(InventoryKey, _inventory.Snapshot());
            if (_progress != null) _save.Store(ProgressKey, _progress.Snapshot());
        }
    }
}
