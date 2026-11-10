using System;
using UnityEngine;

namespace Front21.Thermal
{
    public sealed class ThermalGuard : IThermalGuard, IDisposable
    {
        // Бюджеты кадра на профиль, мс.
        static readonly float[] FrameBudgetMs = { 16.6f, 22.2f, 33.3f };
        // Рендер-скейл на профиль.
        static readonly float[] RenderScale = { 1f, 0.72f, 0.6f };
        // Порог нагрева корпуса на профиль, °C.
        static readonly float[] TempThresholdC = { 40f, 44f, 48f };
        // Кап FPS на профиль.
        static readonly int[] TargetFps = { 60, 45, 30 };

        const float HeatStepUp = 3f;
        const float CoolStepDown = 1.5f;
        const float SampleWindow = 4f;
        const float FrameDropRatio = 1.25f;
        const float DegradeCooldown = 6f;

        readonly IThermalSensor _sensor;
        readonly float[] _frameTimes = new float[64];

        ThermalProfile _profile = ThermalProfile.Full;
        ThermalSample _lastSample;
        int _frameHead;
        float _accum;
        float _cooldownLeft;
        bool _coolMode;
        bool _disposed;

        public ThermalProfile Current => _profile;
        public ThermalSample LastSample => _lastSample;
        public bool CoolMode => _coolMode;

        public event Action<ThermalProfile> ProfileChanged;
        public event Action<ThermalSample> Sampled;

        public ThermalGuard(IThermalSensor sensor)
        {
            _sensor = sensor ?? throw new ArgumentNullException(nameof(sensor));
        }

        public void SetCoolMode(bool enabled)
        {
            _coolMode = enabled;
            Apply(enabled ? ThermalProfile.Low : ThermalProfile.Balanced);
        }

        public void Reset()
        {
            _accum = 0f;
            _cooldownLeft = 0f;
            _frameHead = 0;
            Array.Clear(_frameTimes, 0, _frameTimes.Length);
            Apply(ThermalProfile.Full);
        }

        public void Tick(float deltaTime)
        {
            if (_disposed || deltaTime <= 0f) return;

            _frameTimes[_frameHead % _frameTimes.Length] = deltaTime * 1000f;
            _frameHead++;

            if (_cooldownLeft > 0f) _cooldownLeft -= deltaTime;

            _accum += deltaTime;
            if (_accum < SampleWindow) return;

            Evaluate();
            _accum = 0f;
        }

        void Evaluate()
        {
            float avgMs = AverageFrameMs();
            float temp = _sensor.ReadCelsius();

            _lastSample = new ThermalSample(temp, avgMs, _profile);
            Sampled?.Invoke(_lastSample);

            if (_coolMode)
            {
                if ((int)_profile != (int)ThermalProfile.Low) Apply(ThermalProfile.Low);
                return;
            }

            bool canDegrade = _cooldownLeft <= 0f && (int)_profile < (int)ThermalProfile.Low;
            if (!canDegrade) return;

            float budget = FrameBudgetMs[(int)_profile];
            if (avgMs > budget * FrameDropRatio)
            {
                Degrade($"просадка кадра {avgMs:F1} мс при бюджете {budget:F1} мс");
                return;
            }

            if (temp > 0f && temp >= TempThresholdC[(int)_profile] + HeatStepUp)
            {
                Degrade($"нагрев {temp:F1} °C при пороге {TempThresholdC[(int)_profile]:F1} °C");
                return;
            }

            // Возврат вверх по профилям — только при явном остывании.
            if ((int)_profile > 0 && temp > 0f && temp <= TempThresholdC[(int)_profile] - CoolStepDown)
            {
                Apply((ThermalProfile)((int)_profile - 1));
            }
        }

        float AverageFrameMs()
        {
            int count = Math.Min(_frameHead, _frameTimes.Length);
            if (count == 0) return 0f;

            float sum = 0f;
            for (int i = 0; i < count; i++) sum += _frameTimes[i];
            return sum / count;
        }

        void Degrade(string reason)
        {
            Debug.Log($"[Thermal] понижение профиля: {reason}");
            Apply((ThermalProfile)((int)_profile + 1));
            _cooldownLeft = DegradeCooldown;
        }

        void Apply(ThermalProfile profile)
        {
            profile = _coolMode ? ThermalProfile.Low : profile;
            ProfileChanged?.Invoke(profile);

            if (profile == _profile)
            {
                ApplyQuality();
                return;
            }

            _profile = profile;
            ApplyQuality();
        }

        void ApplyQuality()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps[(int)_profile];

            float scale = RenderScale[(int)_profile];
            ScalableBufferManager.ResizeBuffers(scale, scale);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 1;
            ScalableBufferManager.ResizeBuffers(1f, 1f);

            ProfileChanged = null;
            Sampled = null;
        }
    }
}
