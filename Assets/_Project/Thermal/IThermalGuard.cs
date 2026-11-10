using System;

namespace Front21.Thermal
{
    public enum ThermalProfile
    {
        Full = 0,
        Balanced = 1,
        Low = 2
    }

    public readonly struct ThermalSample
    {
        public readonly float TemperatureCelsius;
        public readonly float FrameMs;
        public readonly ThermalProfile Profile;

        public ThermalSample(float temperatureCelsius, float frameMs, ThermalProfile profile)
        {
            TemperatureCelsius = temperatureCelsius;
            FrameMs = frameMs;
            Profile = profile;
        }
    }

    /// <summary>
    /// Следит за нагревом и просадкой кадра, поэтапно сбрасывает качество.
    /// Профили только понижаются при нагреве, возврат — с гистерезисом.
    /// </summary>
    public interface IThermalGuard
    {
        ThermalProfile Current { get; }
        ThermalSample LastSample { get; }
        bool CoolMode { get; }

        void Tick(float deltaTime);
        void SetCoolMode(bool enabled);
        void Reset();

        event Action<ThermalProfile> ProfileChanged;
        event Action<ThermalSample> Sampled;
    }
}
