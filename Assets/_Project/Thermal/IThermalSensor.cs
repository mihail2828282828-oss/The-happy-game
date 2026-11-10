namespace Front21.Thermal
{
    /// <summary>
    /// Источник температуры корпуса. Возвращает 0, если датчик недоступен,
    /// тогда ThermalGuard работает только по просадке кадра.
    /// </summary>
    public interface IThermalSensor
    {
        float ReadCelsius();
    }
}
