using Front21.Thermal;
using UnityEngine;

namespace Front21.Core
{
    public sealed class DeviceBootstrapper : MonoBehaviour
    {
        [SerializeField] bool _coolModeOnStart;

        ThermalGuard _thermal;
        MemorySaveSystem _save;

        void Awake()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            IThermalSensor sensor = new AndroidBatteryThermalSensor();
#else
            IThermalSensor sensor = new NullThermalSensor();
#endif
            _thermal = new ThermalGuard(sensor);
            _save = new MemorySaveSystem();

            GameServices.Bootstrap(new Economy.Wallet(), _thermal, _save);

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
            if (paused)
            {
                GameServices.Thermal?.Tick(0f);
                GameServices.Save?.Flush();
            }
        }

        void OnDestroy()
        {
            GameServices.Save?.Flush();
            _thermal?.Dispose();
            GameServices.Shutdown();
        }
    }
}
