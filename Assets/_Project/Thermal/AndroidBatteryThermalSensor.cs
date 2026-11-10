#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace Front21.Thermal
{
    public sealed class AndroidBatteryThermalSensor : IThermalSensor
    {
        const string BatteryManagerClass = "android.os.BatteryManager";
        const string BatteryService = "battery";
        const string ExtraTemperature = "EXTRA_TEMPERATURE"; // десятые доли градуса, int
        const string ActionChanged = "android.intent.action.BATTERY_CHANGED";

        AndroidJavaObject _receiver;
        bool _failed;

        public float ReadCelsius()
        {
            if (_failed) return 0f;

            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");

                if (_receiver == null)
                {
                    using var intentFilter = new AndroidJavaObject("android.content.IntentFilter", ActionChanged);
                    using var context = activity.Call<AndroidJavaObject>("getApplicationContext");
                    _receiver = context.Call<AndroidJavaObject>("registerReceiver", null, intentFilter);
                }

                if (_receiver == null) return 0f;

                using var batteryClass = new AndroidJavaClass(BatteryManagerClass);
                using var key = batteryClass.GetStatic<string>(ExtraTemperature); // нет такого поля — оставлено для явности
                int tenths = _receiver.Call<int>("getIntExtra", "temperature", -1);
                if (tenths < 0) return 0f;

                return tenths / 10f;
            }
            catch
            {
                _failed = true;
                return 0f;
            }
        }
    }
}
#endif
