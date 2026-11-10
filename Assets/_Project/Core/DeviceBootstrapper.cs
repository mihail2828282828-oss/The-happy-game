using Front21.Economy;
using Front21.Thermal;
using UnityEngine;

namespace Front21.Core
{
    /// <summary>
    /// Точка входа в сцену: собирает сервисы, восстанавливает кошелёк из хранилища,
    /// гоняет термоконтроль и сохраняет прогресс при паузе и уничтожении.
    /// Живёт на одном объекте в стартовой сцене.
    /// </summary>
    public sealed class DeviceBootstrapper : MonoBehaviour
    {
        const string WalletKey = "wallet";

        [SerializeField] bool _coolModeOnStart;

        ThermalGuard _thermal;
        MemorySaveSystem _save;
        Wallet _wallet;
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
            if (_save.TryLoad(WalletKey, out WalletState state)) _wallet.Restore(state);

            GameServices.Bootstrap(_wallet, _thermal, _save);

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

            PersistWallet();
            GameServices.Save?.Flush();
        }

        void OnApplicationQuit()
        {
            if (_shutdown) return;

            PersistWallet();
            GameServices.Save?.Flush();
        }

        void OnDestroy()
        {
            Shutdown();
        }

        void Shutdown()
        {
            if (_shutdown) return;
            _shutdown = true;

            PersistWallet();
            GameServices.Save?.Flush();

            _thermal?.Dispose();
            GameServices.Shutdown();

            _wallet = null;
            _save = null;
            _thermal = null;
        }

        void PersistWallet()
        {
            if (_wallet == null || _save == null) return;
            _save.Store(WalletKey, _wallet.Snapshot());
        }
    }
}
